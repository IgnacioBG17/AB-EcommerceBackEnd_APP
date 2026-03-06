using Ecommerce.Application.Features.Dashboard.Vms;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSalesByCountry
{
    public class GetSalesByCountryQueryHandler
    : IRequestHandler<GetSalesByCountryQuery, List<SalesByCountryVm>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public GetSalesByCountryQueryHandler(
            IUnitOfWork unitOfWork,
            IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<List<SalesByCountryVm>> Handle(
            GetSalesByCountryQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"dashboard_sales_country_{request.StartDate}_{request.EndDate}";

            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cached))
                return JsonConvert.DeserializeObject<List<SalesByCountryVm>>(cached)!;

            var query = _unitOfWork
                .Repository<Order>()
                .Query()
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed);

            if (request.StartDate.HasValue)
                query = query.Where(o => o.CreateDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(o => o.CreateDate <= request.EndDate.Value);

            var result = await query
                .GroupBy(o => o.OrderAddress.Pais)
                .Select(g => new SalesByCountryVm
                {
                    Pais = g.Key,
                    TotalVentas = g.Sum(x => x.Total),
                    CantidadOrdenes = g.Count()
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
