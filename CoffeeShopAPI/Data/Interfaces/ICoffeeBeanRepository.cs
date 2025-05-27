using CoffeeShopAPI.Models;
using CoffeeShopAPI.Models.RequestTypes;

namespace CoffeeShopAPI.Data.Interfaces
{
    /// <summary>
    /// CoffeeBeanRepository interface
    /// </summary>
    public interface ICoffeeBeanRepository
    {
        public Task<bool> LoadInitialData();

        public Task<List<CoffeeBean>> GetAll();

        public Task<bool> DeleteAll();

        public Task<bool> InsertCoffeeBean(InsertCoffeeBeanRequest request);

        public Task<bool> UpdateCoffeeBean(UpdateCoffeeBeanRequest request);

        public Task<List<CoffeeBean>> Search(SearchCoffeeBeansRequest request);
    }
}
