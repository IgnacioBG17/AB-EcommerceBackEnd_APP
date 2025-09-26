using Ecommerce.Application.Contracts.Infrastructure;
using Ecommerce.Application.Models.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SendEmail()
        {
            var message = new EmailMessage
            {
                To = "angelbustamante72@outlook.com",
                Body = "Esta es una pruena de envio de Email con token",
                Subject = "Cambiar el Password"
            };

            var result = await _emailService.SendEmailAsync(message, "Este_ES_Mi_Token");

            return result ? Ok() : BadRequest();
        }
    }
}
