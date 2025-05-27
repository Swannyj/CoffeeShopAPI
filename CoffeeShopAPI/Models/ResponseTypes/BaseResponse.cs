namespace CoffeeShopAPI.Models.ResponseTypes
{
    public class BaseResponse
    {
        public bool success {  get; set; } 
        public string message { get; set; }

        public BaseResponse() {
            success = true;
            message = "Success"; ;
        }
    }
}
