using LAB_3.Models;
using System.Collections.Generic;

namespace LAB_3.Services.ICachedService 
{
    public interface ICachedEnterprisesService
    {
        public IEnumerable<Enterprise> GetEnterprises(int rowsNumber = 20);
        public void AddEnterprises(string cacheKey, int rowsNumber = 20);
        public IEnumerable<Enterprise> GetEnterprises(string cacheKey, int rowsNumber = 20);
    }
}
