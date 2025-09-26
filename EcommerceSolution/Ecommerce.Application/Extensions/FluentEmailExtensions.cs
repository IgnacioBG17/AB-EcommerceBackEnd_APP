using Ecommerce.Application.Models.Email;
using FluentEmail.Core;
using FluentEmail.Mailgun;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application.Extensions
{
    public static class FluentEmailExtensions
    {
        public static void AddServiceEmail(this IServiceCollection services, 
                                            IConfiguration configuration)
        {
            services.Configure<EmailFluentSettings>(
                configuration.GetSection(nameof(EmailFluentSettings))    
            );

            /* Para Mailgun */
            var emailSettings = configuration.GetSection(nameof(EmailFluentSettings));
            var gunEmailKey = emailSettings.GetValue<string>("GunEmailKey");
            var sender = emailSettings.GetValue<string>("GunEmailSender");
            var smtp = emailSettings.GetValue<string>("GunEmailSmtp");
            var port = emailSettings.GetValue<int>("GunEmailPort");
            var password = emailSettings.GetValue<string>("GunEmailPassword");
            var domain = emailSettings.GetValue<string>("Domain");

            var senderObject = new MailgunSender(domain, gunEmailKey);

            Email.DefaultSender = senderObject;

            services.AddFluentEmail(sender)
                .AddSmtpSender(smtp, port, sender, password);

            /* Para  Smtpv4dev */
            //var fromEmail = emailSettings.GetValue<string>("Email");
            //var host = emailSettings.GetValue<string>("Host");
            //var port = emailSettings.GetValue<int>("Port");

            //services.AddFluentEmail(fromEmail).AddSmtpSender(host, port);
        }
    }
}
