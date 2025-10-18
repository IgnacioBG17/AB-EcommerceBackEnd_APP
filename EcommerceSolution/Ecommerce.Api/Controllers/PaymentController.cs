using Ecommerce.Application.Features.Orders.Vms;
using Ecommerce.Application.Features.Payments.Commands.CreatePayment;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Procesa la confirmación de pago de una orden enviando el comando correspondiente
        /// a la capa de aplicación mediante <c>MediatR</c>. Devuelve la orden resultante
        /// (por ejemplo, marcada como <c>Completed</c> y con las actualizaciones asociadas).
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreatePayment")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<OrderVm>> CreatePayment([FromBody] CreatePaymentCommand request)
        {
            return await _mediator.Send(request);
        }
    }
}
