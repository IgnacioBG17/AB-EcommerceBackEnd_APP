using Stripe;

namespace Ecommerce.Application.Contracts.Stripe
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Crea un PaymentIntent en Stripe y devuelve el objeto PaymentIntent
        /// </summary>
        /// <param name="amountInCents">Monto en centavos (ej: $10.50 => 1050)</param>
        /// <param name="currency">Moneda ISO (ej: "usd")</param>
        /// <param name="metadata">Metadata opcional (orderId, userId, etc.)</param>
        Task<PaymentIntent> CreatePaymentIntentAsync(
            long amountInCents,
            string currency,
            Dictionary<string, string>? metadata = null
        );
        Task<PaymentIntent> UpdatePaymentIntentAmountAsync(string paymentIntentId, long amountInCents);
        Task<PaymentIntent> UpdatePaymentIntentMetadataAsync(string paymentIntentId, Dictionary<string, string> metadata);
    }
}
