using LAB_3.Models;
using System.Collections.Generic;

namespace LAB_3.Services.ICachedService
{
    public interface ICachedSalesPlansService
    {
        public IEnumerable<SalesPlan> GetSalesPlans(int rowsNumber = 20);
        public void AddSalesPlans(string cacheKey, int rowsNumber = 20);
        public IEnumerable<SalesPlan> GetSalesPlans(string cacheKey, int rowsNumber = 20);
    }
}
