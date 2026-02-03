using Ecommerce.Application.Models.Payment;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Ecommerce.Application.Features.Payments.Commands.ProcessStripeWebhook
{
    public class ProcessStripeWebhookCommandHandler : IRequestHandler<ProcessStripeWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<ProcessStripeWebhookCommandHandler> _logger;

        public ProcessStripeWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            IOptions<StripeSettings> stripeSettings,
            ILogger<ProcessStripeWebhookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
        }

        public async Task<Unit> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
        {
            //var webhookSecret = _configuration["StripeSettings:WebhookSecret"];
            var webhookSecret = _stripeSettings.WebhookSecret;
            Event stripeEvent;

            try
            {
                // Validar firma de forma segura
                stripeEvent = EventUtility.ConstructEvent(request.Payload, request.SignatureHeader, webhookSecret, throwOnApiVersionMismatch: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo en la validación del Webhook de Stripe. Verifica el Secret.");
                return Unit.Value;
            }

            // Cast seguro del objeto
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return Unit.Value;

            switch (stripeEvent.Type)
            {
                case EventTypes.PaymentIntentSucceeded:
                    await ProcessSucceededPayment(paymentIntent);
                    break;

                case EventTypes.PaymentIntentPaymentFailed:
                    await ProcessFailedPayment(paymentIntent);
                    break;
            }

            return Unit.Value;
        }

        private async Task ProcessSucceededPayment(PaymentIntent paymentIntent)
        {
            // Extraemos los IDs de la metadata que enviamos en el paso anterior
            paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr);
            paymentIntent.Metadata.TryGetValue("ShoppingCartMasterId", out var cartIdStr);

            // 1. Actualizar Orden a "Completed" (Ya no Pending)
            var orderToPay = await _unitOfWork.Repository<Order>().GetEntityAsync(x => x.PaymentIntentId == paymentIntent.Id);
            orderToPay.Status = OrderStatus.Completed;
            _unitOfWork.Repository<Order>().UpdateEntity(orderToPay);

            // 2. BORRAR EL CARRITO (Justo lo que querías)
            if (Guid.TryParse(cartIdStr, out var cartId))
            {
                var items = await _unitOfWork.Repository<ShoppingCartItem>()
                                    .GetAsync(x => x.ShoppingCartMasterId == cartId);

                _unitOfWork.Repository<ShoppingCartItem>().DeleteRange(items);
            }

            await _unitOfWork.Complete();
        }

        private async Task ProcessFailedPayment(PaymentIntent paymentIntent)
        {
            // Buscamos la orden asociada al PaymentIntent que falló
            var order = await _unitOfWork.Repository<Order>()
                .GetEntityAsync(
                    x => x.PaymentIntentId == paymentIntent.Id,
                    null,
                    false
                );

            if (order == null)
            {
                _logger.LogWarning("No se encontró la orden para el pago fallido. PaymentIntentId: {Id}", paymentIntent.Id);
                return;
            }

            // Solo actualizamos si no estaba ya en estado de error
            if (order.Status != OrderStatus.Error)
            {
                order.Status = OrderStatus.Error;

                // Opcional: Stripe suele enviar la razón del fallo en LastPaymentError
                var reason = paymentIntent.LastPaymentError?.Message ?? "Pago rechazado por Stripe";

                _logger.LogWarning("El pago para la Orden {OrderId} falló. Razón: {Reason}", order.Id, reason);

                _unitOfWork.Repository<Order>().UpdateEntity(order);
                await _unitOfWork.Complete();

                _logger.LogInformation("Orden {OrderId} marcada como Error correctamente.", order.Id);
            }
            else
            {
                _logger.LogInformation("La orden {OrderId} ya estaba marcada como Error.", order.Id);
            }
        }


    }
}
