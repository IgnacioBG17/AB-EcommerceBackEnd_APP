using Ecommerce.Domain;

namespace Ecommerce.Application.Specifications.Orders
{
    public class OrderForCountingSpecification : BaseSpecification<Order>
    {
        public OrderForCountingSpecification(OrderSpecificationParams orderParams)
            : base(
                x => 
                (string.IsNullOrEmpty(orderParams.UserName) || x.CompradorUserName!.Contains(orderParams.UserName)) &&
                (!orderParams.Id.HasValue || x.Id == orderParams.Id)
            )
        {
            
        }
    }
}
