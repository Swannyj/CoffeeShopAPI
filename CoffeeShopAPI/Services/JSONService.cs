using Microsoft.IdentityModel.Tokens;
using CoffeeShopAPI.Data.Interfaces;
using CoffeeShopAPI.Models;
using System.Text.Json;
using CoffeeShopAPI.Data;

namespace CoffeeShopAPI.services
{
    /// <summary>
    /// JSON Service
    /// </summary>
    public class JSONService: IJSONService
    {
        #region Global Variables

        private readonly ILogger<JSONService> _logger;
        private readonly AppDbContext _dbContext;

        #endregion Global Variables

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        public JSONService(AppDbContext dbContext, ILogger<JSONService> logger) { 
            _logger = logger;
            _dbContext = dbContext;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Load data from file location
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task LoadDataFromFile(String path)
        {
            // If the file cannot be find, throw exception
            if (!File.Exists(path))
                throw new FileNotFoundException("JSON file not found", path);

            try
            {
                var json = File.ReadAllText(path);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Allows matching JSON keys regardless of casing
                };

                // Deserialize into Coffeebeans
                List<CoffeeBean> coffeeBeans = JsonSerializer.Deserialize<List<CoffeeBean>>(json, options);

                // If coffeeBeans is null, return false
                if (coffeeBeans.IsNullOrEmpty())
                    _logger.LogInformation("No data found in json file");

                // Add all
                await _dbContext.CoffeeBean.AddRangeAsync(coffeeBeans);

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "An unhandled error occurred while trying to import data from json file.");
            }
        }

        #endregion Public Methods
    }
}