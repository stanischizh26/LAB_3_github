using LAB_3.Models;
using LAB_3.Services.ICachedService;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LAB_3.Services.CachedService
{
    public class CachedProductsService(ProductionContext dbContext, IMemoryCache memoryCache) : ICachedProductsService
    {
        private readonly ProductionContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // получение списка емкостей из базы
        public IEnumerable<Product> GetProducts(int rowsNumber = 20)
        {
            return _dbContext.Products.Take(rowsNumber).ToList();
        }

        // добавление списка емкостей в кэш
        public void AddProducts(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Product> products = _dbContext.Products.Take(rowsNumber).ToList();
            if (products != null)
            {
                _memoryCache.Set(cacheKey, products, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            }

        }
        // получение списка емкостей из кэша или из базы, если нет в кэше
        public IEnumerable<Product> GetProducts(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Product> products))
            {
                products = _dbContext.Products.Take(rowsNumber).ToList();
                if (products != null)
                {
                    _memoryCache.Set(cacheKey, products,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }
            }
            return products;
        }
        public Product SearchObj(string name)
        {
            return _dbContext.Products.FirstOrDefault(p => p.Name == name);
        }
    }
}

