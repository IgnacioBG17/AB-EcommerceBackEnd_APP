using Ecommerce.Application.Contracts.Stripe;
using Ecommerce.Application.Models.Payment;
using Microsoft.Extensions.Options;
using Stripe;

namespace Ecommerce.Infrastructure.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly StripeSettings _stripeSettings;
        public StripePaymentService(IOptions<StripeSettings> stripeOptions)
        {
            _stripeSettings = stripeOptions.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }
        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amountInCents, string currency, Dictionary<string, string>? metadata = null)
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = metadata
            };

            return await service.CreateAsync(options);
        }

        public async Task<PaymentIntent> UpdatePaymentIntentAmountAsync(string paymentIntentId, long amountInCents)
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentUpdateOptions
            {
                Amount = amountInCents
            };
            return await service.UpdateAsync(paymentIntentId, options);
        }

        public async Task<PaymentIntent> UpdatePaymentIntentMetadataAsync(string paymentIntentId, Dictionary<string, string> metadata)
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentUpdateOptions
            {
                Metadata = metadata
            };
            return await service.UpdateAsync(paymentIntentId, options);
        }
    }
}
