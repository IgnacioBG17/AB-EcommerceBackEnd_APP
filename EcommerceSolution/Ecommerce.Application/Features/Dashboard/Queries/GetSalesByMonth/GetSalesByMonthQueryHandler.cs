using Ecommerce.Application.Features.Dashboard.Vms;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Ecommerce.Application.Features.Dashboard.Queries.GetSalesByMonth
{
    public class GetSalesByMonthQueryHandler
    : IRequestHandler<GetSalesByMonthQuery, List<SalesByMonthVm>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public GetSalesByMonthQueryHandler(
            IUnitOfWork unitOfWork,
            IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<List<SalesByMonthVm>> Handle(
            GetSalesByMonthQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"dashboard_sales_month_{request.StartDate}_{request.EndDate}";

            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cached))
                return JsonConvert.DeserializeObject<List<SalesByMonthVm>>(cached)!;

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
                .GroupBy(o => new { o.CreateDate.Year, o.CreateDate.Month })
                .Select(g => new SalesByMonthVm
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
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
