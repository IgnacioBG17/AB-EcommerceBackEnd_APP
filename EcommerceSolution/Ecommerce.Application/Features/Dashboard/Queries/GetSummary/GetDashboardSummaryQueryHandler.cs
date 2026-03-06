using Ecommerce.Application.Features.Dashboard.Vms;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSummary
{
    public class GetDashboardSummaryQueryHandler
        : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryVm>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public GetDashboardSummaryQueryHandler(
            IUnitOfWork unitOfWork,
            IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<DashboardSummaryVm> Handle(
            GetDashboardSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"dashboard_summary_{request.StartDate}_{request.EndDate}";

            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonConvert.DeserializeObject<DashboardSummaryVm>(cachedData)!;
            }

            var ordersQuery = _unitOfWork
                .Repository<Order>()
                .Query()
                .AsNoTracking();

            var completedQuery = ordersQuery
                .Where(o => o.Status == OrderStatus.Completed);

            if (request.StartDate.HasValue)
                completedQuery = completedQuery
                    .Where(o => o.CreateDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                completedQuery = completedQuery
                    .Where(o => o.CreateDate <= request.EndDate.Value);

            var totalVentas = await completedQuery
                .SumAsync(o => o.Total, cancellationToken);

            var totalOrdenes = await ordersQuery
                .CountAsync(cancellationToken);

            var pendientes = await ordersQuery
                .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

            var enviadas = await ordersQuery
                .CountAsync(o => o.Status == OrderStatus.Enviado, cancellationToken);

            var totalProductos = await _unitOfWork
                .Repository<Product>()
                .Query()
                .CountAsync(cancellationToken);

            var result = new DashboardSummaryVm
            {
                TotalVentas = totalVentas,
                TotalOrdenes = totalOrdenes,
                OrdenesPendientes = pendientes,
                OrdenesEnviadas = enviadas,
                TotalProductos = totalProductos
            };

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(result),
                options,
                cancellationToken);

            return result;
        }
    }
}