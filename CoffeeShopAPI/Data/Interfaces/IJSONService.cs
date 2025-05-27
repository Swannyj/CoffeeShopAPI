namespace CoffeeShopAPI.Data.Interfaces
{
    /// <summary>
    /// JSONService interface
    /// </summary>
    public interface IJSONService
    {
        public Task LoadDataFromFile(string path);
    }
}
