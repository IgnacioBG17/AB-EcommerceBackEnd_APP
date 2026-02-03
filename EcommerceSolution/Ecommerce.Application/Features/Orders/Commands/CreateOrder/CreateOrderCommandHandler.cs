using AutoMapper;
using Ecommerce.Application.Contracts.Identity;
using Ecommerce.Application.Contracts.Stripe;
using Ecommerce.Application.Features.Orders.Vms;
using Ecommerce.Application.Models.Payment;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Ecommerce.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderVm>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly UserManager<Usuario> _userManager;
        private readonly StripeSettings _stripeSettings;
        private readonly IStripePaymentService _stripePaymentService;

        public CreateOrderCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAuthService authService,
            UserManager<Usuario> userManager,
            IOptions<StripeSettings> stripeSettings,
            IStripePaymentService stripePaymentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authService = authService;
            _userManager = userManager;
            _stripeSettings = stripeSettings.Value;
            _stripePaymentService = stripePaymentService;
        }

        public async Task<OrderVm> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var username = _authService.GetSessionUser();

            // 1️. Obtener carrito con items
            var includes = new List<Expression<Func<ShoppingCart, object>>>
            {
                c => c.ShoppingCartItems!.OrderBy(i => i.Producto)
            };

            var shoppingCart = await _unitOfWork.Repository<ShoppingCart>()
                .GetEntityAsync(x => x.ShoppingCartMasterId == request.ShoppingCartId, includes, false);

            if (shoppingCart == null || !shoppingCart.ShoppingCartItems!.Any())
                throw new Exception("El carrito está vacío");

            // 2️. Usuario autenticado
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
                throw new Exception("Usuario no autenticado");

            // 3️. Calcular totales
            var subTotal = Math.Round(shoppingCart.ShoppingCartItems!.Sum(x => x.Precio * x.Cantidad), 2);
            var impuesto = Math.Round(subTotal * 0.18m, 2);
            var precioEnvio = subTotal < 100 ? 10 : 25;
            var total = subTotal + impuesto + precioEnvio;

            // 4️. Buscar orden pendiente (NO BORRARLA)
            var order = await _unitOfWork.Repository<Order>().GetEntityAsync(
                x => x.CompradorUserName == username && x.Status == OrderStatus.Pending,
                null,
                true
            );

            // 5️. Dirección
            var direccion = await _unitOfWork.Repository<Domain.Address>()
                .GetEntityAsync(x => x.UserName == username, null, false);

            if (direccion == null)
                throw new Exception("Dirección no encontrada");

            // 6️. Crear o actualizar orden
            if (order == null)
            {
                var orderAddress = new OrderAddress
                {
                    Direccion = direccion.Direccion,
                    Ciudad = direccion.Ciudad,
                    CodigoPostal = direccion.CodigoPostal,
                    Pais = direccion.Pais,
                    Departamento = direccion.Departamento,
                    UserName = direccion.UserName
                };

                await _unitOfWork.Repository<OrderAddress>().AddAsync(orderAddress);

                var nombreComprador = $"{user.Nombre} {user.Apellido}";

                order = new Order(
                    nombreComprador,
                    username,
                    orderAddress,
                    subTotal,
                    total,
                    impuesto,
                    precioEnvio
                );

                await _unitOfWork.Repository<Order>().AddAsync(order);
            }
            else
            {
                // Actualizar totales (idealmente método de dominio)
                order.SubTotal = subTotal;
                order.Total = total;
                order.Impuesto = impuesto;
                order.PrecioEnvio = precioEnvio;

                // Limpiar items previos
                var oldItems = await _unitOfWork.Repository<OrderItem>()
                    .GetAsync(x => x.OrderId == order.Id);

                _unitOfWork.Repository<OrderItem>().DeleteRange(oldItems);
            }

            // 7️. Crear items de orden
            var items = shoppingCart.ShoppingCartItems.Select(i => new OrderItem
            {
                ProductNombre = i.Producto,
                ProductId = i.ProductId,
                ImagenUrl = i.Imagen!,
                Precio = i.Precio,
                Cantidad = i.Cantidad,
                OrderId = order.Id
            }).ToList();

            _unitOfWork.Repository<OrderItem>().AddRange(items);

            var result = await _unitOfWork.Complete();
            if (result <= 0)
                throw new Exception("Error guardando la orden");

            // 8️. Stripe (crear o actualizar PaymentIntent)
            var metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() },
                { "ShoppingCartMasterId", shoppingCart.ShoppingCartMasterId.ToString()! }
            };

            var amountInCents = (long)Math.Round(order.Total * 100);

            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                var intent = await _stripePaymentService
                    .CreatePaymentIntentAsync(amountInCents, "usd", metadata);

                order.PaymentIntentId = intent.Id;
                order.ClientSecret = intent.ClientSecret;
                order.StripeApiKey = _stripeSettings.Publishblekey;
            }
            else
            {
                await _stripePaymentService
                    .UpdatePaymentIntentAmountAsync(order.PaymentIntentId, amountInCents);
            }

            _unitOfWork.Repository<Order>().UpdateEntity(order);
            await _unitOfWork.Complete();

            return _mapper.Map<OrderVm>(order);
        }
    }
}
