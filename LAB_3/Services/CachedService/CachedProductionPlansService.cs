using LAB_3.Models;
using LAB_3.Services.ICachedService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LAB_3.Services.CachedService
{
    public class CachedProductionPlansService(ProductionContext dbContext, IMemoryCache memoryCache) : ICachedProductionPlansService
    {
        private readonly ProductionContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // получение списка емкостей из базы
        public IEnumerable<ProductionPlan> GetProductionPlans(int rowsNumber = 20)
        {
            return _dbContext.ProductionPlans.Include(e => e.Enterprise).Include(p => p.Product).Take(rowsNumber).ToList();
        }

        // добавление списка емкостей в кэш
        public void AddProductionPlans(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<ProductionPlan> productionPlans = _dbContext.ProductionPlans.Include(e => e.Enterprise).Include(p => p.Product).Take(rowsNumber).ToList();
            if (productionPlans != null)
            {
                _memoryCache.Set(cacheKey, productionPlans, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            }

        }
        // получение списка емкостей из кэша или из базы, если нет в кэше
        public IEnumerable<ProductionPlan> GetProductionPlans(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<ProductionPlan> productionPlans))
            {
                productionPlans = _dbContext.ProductionPlans.Include(e => e.Enterprise).Include(p => p.Product).Take(rowsNumber).ToList();
                if (productionPlans != null)
                {
                    _memoryCache.Set(cacheKey, productionPlans,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }
            }
            return productionPlans;
        }

    }
}

