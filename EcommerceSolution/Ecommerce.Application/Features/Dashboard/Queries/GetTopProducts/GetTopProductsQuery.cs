using Ecommerce.Application.Features.Dashboard.Vms;
using MediatR;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetTopProducts
{
        public record GetTopProductsQuery(
        DateTime? StartDate,
        DateTime? EndDate
    ) : IRequest<List<TopProductVm>>;
}
