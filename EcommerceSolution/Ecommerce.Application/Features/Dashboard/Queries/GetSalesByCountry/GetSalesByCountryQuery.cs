using Ecommerce.Application.Features.Dashboard.Vms;
using MediatR;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSalesByCountry
{
        public record GetSalesByCountryQuery(
        DateTime? StartDate,
        DateTime? EndDate
    ) : IRequest<List<SalesByCountryVm>>;
}
