using Ecommerce.Application.Features.Dashboard.Vms;
using MediatR;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSalesByMonth
{
    public record GetSalesByMonthQuery(
    DateTime? StartDate,
    DateTime? EndDate
    ) : IRequest<List<SalesByMonthVm>>;
}



