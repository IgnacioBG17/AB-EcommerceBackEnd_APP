using Ecommerce.Application.Features.Dashboard.Vms;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetTopProducts
{
    public class GetTopProductsQueryHandler
    : IRequestHandler<GetTopProductsQuery, List<TopProductVm>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public GetTopProductsQueryHandler(
            IUnitOfWork unitOfWork,
            IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<List<TopProductVm>> Handle(
            GetTopProductsQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"dashboard_top_products_{request.StartDate}_{request.EndDate}";

            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cached))
                return JsonConvert.DeserializeObject<List<TopProductVm>>(cached)!;

            var ordersQuery = _unitOfWork
                .Repository<Order>()
                .Query()
                .Where(o => o.Status == OrderStatus.Completed);

            if (request.StartDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreateDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreateDate <= request.EndDate.Value);

            var result = await ordersQuery
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.ProductId, oi.ProductNombre })
                .Select(g => new TopProductVm
                {
                    ProductId = g.Key.ProductId,
                    Nombre = g.Key.ProductNombre!,
                    CantidadVendida = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(5)
                .ToListAsync(cancellationToken);

            await _cache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                },
                cancellationToken);

            return result;
        }
    }
}
