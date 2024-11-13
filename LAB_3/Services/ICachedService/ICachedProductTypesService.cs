using LAB_3.Models;
using System.Collections.Generic;

namespace LAB_3.Services.ICachedService
{
    public interface ICachedProductTypesService
    {
        public IEnumerable<ProductType> GetProductTypes(int rowsNumber = 20);
        public void AddProductTypes(string cacheKey, int rowsNumber = 20);
        public IEnumerable<ProductType> GetProductTypes(string cacheKey, int rowsNumber = 20);
    }
}
