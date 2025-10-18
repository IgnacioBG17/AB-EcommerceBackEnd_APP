using Ecommerce.Application.Features.Orders.Vms;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Queries
{
    public class GetOrdersByIdQuery : IRequest<OrderVm>
    {
        public int OrderId { get; set; }

        public GetOrdersByIdQuery(int orderId)
        {
            OrderId = orderId == 0 ? throw new ArgumentException(nameof(orderId)) : orderId;
        }
    }
}
