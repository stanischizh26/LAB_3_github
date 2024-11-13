using LAB_3.Models;
using LAB_3.Services.ICachedService;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LAB_3.Services.CachedService
{
    public class CachedEnterprisesService(ProductionContext dbContext, IMemoryCache memoryCache) : ICachedEnterprisesService
    {
        private readonly ProductionContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // получение списка емкостей из базы
        public IEnumerable<Enterprise> GetEnterprises(int rowsNumber = 20)
        {
            return _dbContext.Enterprises.Take(rowsNumber).ToList();
        }

        // добавление списка емкостей в кэш
        public void AddEnterprises(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Enterprise> products = _dbContext.Enterprises.Take(rowsNumber).ToList();
            if (products != null)
            {
                _memoryCache.Set(cacheKey, products, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            }

        }
        // получение списка емкостей из кэша или из базы, если нет в кэше
        public IEnumerable<Enterprise> GetEnterprises(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Enterprise> enterprises))
            {
                enterprises = _dbContext.Enterprises.Take(rowsNumber).ToList();
                if (enterprises != null)
                {
                    _memoryCache.Set(cacheKey, enterprises,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }
            }
            return enterprises;
        }

    }
}

