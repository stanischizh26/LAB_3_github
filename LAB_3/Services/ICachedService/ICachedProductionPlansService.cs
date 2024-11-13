using LAB_3.Models;
using System.Collections.Generic;

namespace LAB_3.Services.ICachedService
{
    public interface ICachedProductionPlansService
    {
        public IEnumerable<ProductionPlan> GetProductionPlans(int rowsNumber = 20);
        public void AddProductionPlans(string cacheKey, int rowsNumber = 20);
        public IEnumerable<ProductionPlan> GetProductionPlans(string cacheKey, int rowsNumber = 20);
    }
}
