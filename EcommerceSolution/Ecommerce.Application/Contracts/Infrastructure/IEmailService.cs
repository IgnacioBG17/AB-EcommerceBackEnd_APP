using Ecommerce.Application.Models.Email;

namespace Ecommerce.Application.Contracts.Infrastructure
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage email, string token);
        Task<bool> SendEmailSengridAsync(EmailMessage email, string token);
    }
}
