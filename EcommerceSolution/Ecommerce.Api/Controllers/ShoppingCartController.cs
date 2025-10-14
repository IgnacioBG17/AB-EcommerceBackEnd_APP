using Ecommerce.Application.Features.ShoppingCarts.Commands.DeleteShoppingCartItem;
using Ecommerce.Application.Features.ShoppingCarts.Commands.UpdateShoppingCart;
using Ecommerce.Application.Features.ShoppingCarts.Queries.GetShoppingCartById;
using Ecommerce.Application.Features.ShoppingCarts.Vms;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ShoppingCartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// Obtiene el carrito de compras asociado al identificador especificado.
        /// Si el identificador es vacío (<see cref="Guid.Empty"/>), genera un nuevo carrito
        /// y devuelve su estructura inicial, un objeto <see cref="ShoppingCartVm"/> con la información del carrito existente
        /// o un nuevo carrito vacío si no se encontró ninguno.
        /// </summary>
        /// <returns>
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetShoppingCart")]
        [ProducesResponseType(typeof(ShoppingCartVm), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCartVm>> GetShoppingCart(Guid id)
        {
            var shoppingCartId = id == Guid.Empty ? Guid.NewGuid() : id;
            var query = new GetShoppingCartByIdQuery(shoppingCartId);
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Este método actualiza el carrito de compras de un usuario.
        //  Elimina los productos anteriores y agrega los nuevos que vienen en la petición.
        //  (Reemplaza completamente los ítems del carrito con los nuevos productos seleccionados.)
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("{id}", Name = "UpdateShoppingCart")]
        [ProducesResponseType(typeof(ShoppingCartVm), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCartVm>> UpdateShoppingCart(Guid id, UpdateShoppingCartCommand request)
        {
            request.ShoppingCartId = id;
            return await _mediator.Send(request);
        }

        /// <summary>
        /// Elimina un ítem del carrito de compras identificado en el comando
        /// y devuelve el carrito actualizado con sus ítems ordenados por producto.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpDelete("item/{id}", Name = "DeleteShoppingCart")]
        [ProducesResponseType(typeof(ShoppingCartVm), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCartVm>> DeleteShoppingCart(int id)
        {
            return await _mediator.Send(new DeleteShoppingCartItemCommand() { Id = id});
        }
    }
}
