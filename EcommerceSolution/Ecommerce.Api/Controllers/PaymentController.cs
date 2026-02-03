using Ecommerce.Application.Features.Orders.Vms;
using Ecommerce.Application.Features.Payments.Commands.CreatePayment;
using Ecommerce.Application.Features.Payments.Commands.ProcessStripeWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        /// Punto de enlace (Webhook) para recibir notificaciones de eventos desde Stripe (Pagos).
        /// </summary>
        /// <remarks>
        /// El endpoint lee el cuerpo de la petición en formato crudo (raw JSON) y extrae la firma 
        /// de seguridad del encabezado para validar la autenticidad del evento antes de procesarlo.
        /// </remarks>
        /// <returns>Devuelve un <c>200 OK</c> para confirmar a Stripe que el evento fue recibido.</returns>
        [AllowAnonymous]
        [HttpPost("webhook", Name = "WebHook")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
                return BadRequest("Falta el header de firma de Stripe.");

            await _mediator.Send(new ProcessStripeWebhookCommand(
                json,
                signatureHeader
            ));

            return Ok();
        }

    }
}
