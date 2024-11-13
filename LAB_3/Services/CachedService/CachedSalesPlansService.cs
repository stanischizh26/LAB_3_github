using LAB_3.Models;
using LAB_3.Services.ICachedService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LAB_3.Services.CachedService
{
    public class CachedSalesPlansService(ProductionContext dbContext, IMemoryCache memoryCache) : ICachedSalesPlansService
    {
        private readonly ProductionContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // получение списка емкостей из базы
        public IEnumerable<SalesPlan> GetSalesPlans(int rowsNumber = 20)
        {
            return _dbContext.SalesPlans.Include(p => p.Product).Include(e => e.Enterprise).Take(rowsNumber).ToList();
        }

        // добавление списка емкостей в кэш
        public void AddSalesPlans(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<SalesPlan> salesPlans = _dbContext.SalesPlans.Include(p => p.Product).Include(e => e.Enterprise).Take(rowsNumber).ToList();
            if (salesPlans != null)
            {
                _memoryCache.Set(cacheKey, salesPlans, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            }

        }
        // получение списка емкостей из кэша или из базы, если нет в кэше
        public IEnumerable<SalesPlan> GetSalesPlans(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<SalesPlan> salesPlans))
            {
                salesPlans = _dbContext.SalesPlans.Include(p => p.Product).Include(e => e.Enterprise).Take(rowsNumber).ToList();
                if (salesPlans != null)
                {
                    _memoryCache.Set(cacheKey, salesPlans,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }
            }
            return salesPlans;
        }

    }
}

