using LAB_3.Models;
using LAB_3.Services.ICachedService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LAB_3.Services.CachedService
{
    public class CachedProductTypesService(ProductionContext dbContext, IMemoryCache memoryCache) : ICachedProductTypesService
    {
        private readonly ProductionContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // получение списка емкостей из базы
        public IEnumerable<ProductType> GetProductTypes(int rowsNumber = 20)
        {
            return _dbContext.ProductTypes.Include(p=>p.Product).Take(rowsNumber).ToList();
        }

        // добавление списка емкостей в кэш
        public void AddProductTypes(string cacheKey, int rowsNumber = 20) 
        {
            IEnumerable<ProductType> productTypes = _dbContext.ProductTypes.Include(p => p.Product).Take(rowsNumber).ToList();
            if (productTypes != null)
            {
                _memoryCache.Set(cacheKey, productTypes, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            }

        }
        // получение списка емкостей из кэша или из базы, если нет в кэше
        public IEnumerable<ProductType> GetProductTypes(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<ProductType> productTypes))
            {
                productTypes = _dbContext.ProductTypes.Include(p => p.Product).Take(rowsNumber).ToList();
                if (productTypes != null)
                {
                    _memoryCache.Set(cacheKey, productTypes,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }
            }
            return productTypes;
        }

    }
}

