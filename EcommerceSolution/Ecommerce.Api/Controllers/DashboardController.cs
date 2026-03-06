using Ecommerce.Application.Features.Dashboard.Queries.GetSalesByCategory;
using Ecommerce.Application.Features.Dashboard.Queries.GetSalesByMonth;
using Ecommerce.Application.Features.Dashboard.Queries.GetSummary;
using Ecommerce.Application.Features.Dashboard.Queries.GetTopProducts;
using Ecommerce.Application.Features.Dashboard.Vms;
using Ecommerce.Application.Models.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = Role.ADMIN)]
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryVm), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<DashboardSummaryVm>> GetSummary(DateTime? startDate, DateTime? endDate)
        {
            return Ok(await _mediator.Send(new GetDashboardSummaryQuery(startDate, endDate)));
        }

        [Authorize(Roles = Role.ADMIN)]
        [HttpGet("sales-by-month")]
        [ProducesResponseType(typeof(List<SalesByMonthVm>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<SalesByMonthVm>>> GetSalesByMonth(DateTime? startDate, DateTime? endDate)
        {
            return Ok(await _mediator.Send(new GetSalesByMonthQuery(startDate, endDate)));
        }

        [Authorize(Roles = Role.ADMIN)]
        [HttpGet("top-products")]
        [ProducesResponseType(typeof(List<TopProductVm>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<TopProductVm>>> GetTopProducts(DateTime? startDate, DateTime? endDate)
        {
            return Ok(await _mediator.Send(new GetTopProductsQuery(startDate, endDate)));
        }

        [Authorize(Roles = Role.ADMIN)]
        [HttpGet("sales-by-category")]
        public async Task<IActionResult> GetSalesByCategory(DateTime? startDate, DateTime? endDate)
        {
            return Ok(await _mediator.Send(new GetSalesByCategoryQuery(startDate, endDate)));
        }

    }
}
