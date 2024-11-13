using LAB_3.Models;
using System.Collections.Generic;

namespace LAB_3.Services.ICachedService
{
    public interface ICachedProductsService
    {
        public IEnumerable<Product> GetProducts(int rowsNumber = 20);
        public void AddProducts(string cacheKey, int rowsNumber = 20);
        public IEnumerable<Product> GetProducts(string cacheKey, int rowsNumber = 20);
        public Product SearchObj(string name);
    }
}
