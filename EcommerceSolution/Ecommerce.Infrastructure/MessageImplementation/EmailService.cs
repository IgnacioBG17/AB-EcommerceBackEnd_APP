using Ecommerce.Application.Contracts.Infrastructure;
using Ecommerce.Application.Models.Email;
using Ecommerce.Domain;
using FluentEmail.Core;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace Ecommerce.Infrastructure.MessageImplementation
{
    public class EmailService : IEmailService
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly EmailFluentSettings _emailFluentSettings;
        private readonly EmailSendGridSettings _emailSendGridSettings;
        public EmailService(IFluentEmail fluentEmail, 
                            IOptions<EmailFluentSettings> emailFluentSettings,
                            IOptions<EmailSendGridSettings> emailSendGridSettings)
        {
            _fluentEmail = fluentEmail;
            _emailFluentSettings = emailFluentSettings.Value;
            _emailSendGridSettings = emailSendGridSettings.Value;
        }

        public async Task<bool> SendEmailAsync(EmailMessage email, string token)
        {
            var htmlContent = $"{email.Body} {_emailFluentSettings.BaseUrlClient}/password/reset/{token}";

            var result = await _fluentEmail
                .To(email.To)
                .Subject(email.Subject)
                .Body(htmlContent)
                .SendAsync();

            return result.Successful;
        }

        public async Task<bool> SendEmailSengridAsync(EmailMessage email, string token)
        {
            var htmlContent = $@"
            <div style=""font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;"">
                <h2 style=""color: #333; text-align: center;"">Restablecer tu contraseña</h2>
                <p style=""color: #555; font-size: 16px; line-height: 1.5;"">
                    Hola, <br><br>
                    Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>{_emailSendGridSettings.FromName}</strong>. 
                    Si no realizaste esta solicitud, puedes ignorar este correo de forma segura.
                </p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{_emailSendGridSettings.BaseUrlClient}/password/reset/{token}"" 
                       style=""background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; font-weight: bold; border-radius: 5px; display: inline-block;"">
                        Restablecer Contraseña
                    </a>
                </div>
                <p style=""color: #777; font-size: 14px;"">
                    Si el botón de arriba no funciona, copia y pega el siguiente enlace en tu navegador:
                </p>
                <p style=""word-break: break-all; color: #007bff; font-size: 12px;"">
                    {_emailSendGridSettings.BaseUrlClient}/password/reset/{token}
                </p>
                <hr style=""border: 0; border-top: 1px solid #eee; margin: 20px 0;"">
                <p style=""color: #999; font-size: 12px; text-align: center;"">
                    &copy; {DateTime.Now.Year} {_emailSendGridSettings.FromName}. Todos los derechos reservados.
                </p>
            </div>";

            var client = new SendGridClient(_emailSendGridSettings.ApiKey);

            var subject = email.Subject;
            var to = new EmailAddress(email.To);
            var emailBody = htmlContent;

            var from = new EmailAddress
            {
                Email = _emailSendGridSettings.Email,
                Name = _emailSendGridSettings.FromName
            };

            var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);
            var response = await client.SendEmailAsync(sendGridMessage);

            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> SendOrderInvoiceSengridAsync(Order order, byte[] pdfInvoice)
        {
            var client = new SendGridClient(_emailSendGridSettings.ApiKey);

            var subject = $"Factura de compra #{order.Id} - Ecommerce";
            var to = new EmailAddress(order.CompradorEmail); 

            var htmlContent = $@"
            <h1 style='color: #333;'>¡Gracias por tu compra, {order.CompradorNombre}!</h1>
            <p>Tu pago ha sido procesado exitosamente.</p>
            <p>Adjunto a este correo encontrarás la factura oficial de tu pedido <strong>#{order.Id}</strong>.</p>
            <br>
            <p>Saludos,<br>El equipo de {(_emailSendGridSettings.FromName ?? "Ecommerce")}</p>";

            var from = new EmailAddress
            {
                Email = _emailSendGridSettings.Email,
                Name = _emailSendGridSettings.FromName
            };

            var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

            sendGridMessage.AddAttachment(
                $"Factura_{order.Id}.pdf",
                Convert.ToBase64String(pdfInvoice),
                "application/pdf"
            );

            var response = await client.SendEmailAsync(sendGridMessage);

            return response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK;
        }
    }
}
