using Ecommerce.Application.Features.Dashboard.Vms;
using MediatR;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSummary
{
    public record GetDashboardSummaryQuery(
    DateTime? StartDate,
    DateTime? EndDate
    ) : IRequest<DashboardSummaryVm>;
}
