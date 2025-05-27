namespace CoffeeShopAPI.Models.ResponseTypes
{
    public class CoffeeBeanResponse : BaseResponse
    {
        public List<CoffeeBean> coffeeBeans { get; set; }

        public CoffeeBeanResponse() { 
            this.coffeeBeans = new List<CoffeeBean>();
        }
    }
}
