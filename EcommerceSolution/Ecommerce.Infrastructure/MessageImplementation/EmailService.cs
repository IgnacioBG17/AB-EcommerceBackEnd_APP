using Ecommerce.Application.Contracts.Infrastructure;
using Ecommerce.Application.Models.Email;
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
            var htmlContent = $"{email.Body} {_emailSendGridSettings.BaseUrlClient}/password/reset/{token}";
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
    }
}
