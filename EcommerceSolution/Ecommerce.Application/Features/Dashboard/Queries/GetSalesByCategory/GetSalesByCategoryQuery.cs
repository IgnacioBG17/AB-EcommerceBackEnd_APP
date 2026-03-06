using Ecommerce.Application.Features.Dashboard.Vms;
using MediatR;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSalesByCategory
{
    public record GetSalesByCategoryQuery(
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<List<SalesByCategoryVm>>;
}
