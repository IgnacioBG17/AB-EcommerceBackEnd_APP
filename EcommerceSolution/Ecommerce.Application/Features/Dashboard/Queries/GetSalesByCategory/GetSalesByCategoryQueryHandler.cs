using Ecommerce.Application.Features.Dashboard.Vms;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSalesByCategory
{
    public class GetSalesByCategoryQueryHandler
     : IRequestHandler<GetSalesByCategoryQuery, List<SalesByCategoryVm>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public GetSalesByCategoryQueryHandler(
            IUnitOfWork unitOfWork,
            IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<List<SalesByCategoryVm>> Handle(
            GetSalesByCategoryQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"dashboard_sales_category_{request.StartDate}_{request.EndDate}";

            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cached))
                return JsonConvert.DeserializeObject<List<SalesByCategoryVm>>(cached)!;

            var ordersQuery = _unitOfWork
                .Repository<Order>()
                .Query()
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed);

            if (request.StartDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreateDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreateDate <= request.EndDate.Value);

            var result = await ordersQuery
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product.Category!.Nombre!)
                .Select(g => new SalesByCategoryVm
                {
                    Categoria = g.Key,
                    TotalVentas = g.Sum(x => x.Precio * x.Cantidad),
                    CantidadVendida = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.TotalVentas)
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
