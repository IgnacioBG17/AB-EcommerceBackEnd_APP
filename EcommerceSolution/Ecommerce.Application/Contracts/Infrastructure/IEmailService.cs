using Ecommerce.Application.Models.Email;
using Ecommerce.Domain;

namespace Ecommerce.Application.Contracts.Infrastructure
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage email, string token);
        Task<bool> SendEmailSengridAsync(EmailMessage email, string token);
        Task<bool> SendOrderInvoiceSengridAsync(Order order, byte[] pdfInvoice);
    }
}
