namespace CoffeeShopAPI.Models.RequestTypes
{
    public class SearchCoffeeBeansRequest
    {
        public string? Name { get; set; }

        public string? Colour { get; set; }

        public string? Cost { get; set; }
    }
}
