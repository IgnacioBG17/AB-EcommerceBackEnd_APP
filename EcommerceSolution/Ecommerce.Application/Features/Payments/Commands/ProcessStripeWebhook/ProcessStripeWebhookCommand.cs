using MediatR;

namespace Ecommerce.Application.Features.Payments.Commands.ProcessStripeWebhook
{
    public class ProcessStripeWebhookCommand : IRequest
    {
        public string Payload { get; }
        public string SignatureHeader { get; }

        public ProcessStripeWebhookCommand(string payload, string signatureHeader)
        {
            Payload = payload;
            SignatureHeader = signatureHeader;
        }
    }
}
