using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using Azure.Core;
using CoffeeShopAPI.Controllers;
using CoffeeShopAPI.Data.Interfaces;
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Models.RequestTypes;
using CoffeeShopAPI.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeShopAPI.Data.Repositories
{
    /// <summary>
    /// CoffeeBeanRespository
    /// </summary>
    public class CoffeeBeanRepository : ICoffeeBeanRepository
    {
        #region Global Variables

        private readonly AppDbContext _dbContext;
        private readonly IJSONService _jsonService;
        private readonly ILogger<CoffeeBeanRepository> _logger;

        #endregion Global Variables

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="jsonService"></param>
        /// <param name="logger"></param>
        public CoffeeBeanRepository(AppDbContext dbContext, IJSONService jsonService, ILogger<CoffeeBeanRepository> logger) {
            _dbContext = dbContext;
            _jsonService = jsonService;
            _logger = logger;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Load Initial data from json file
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoadInitialData() {
            try
            {
                // Make sure there are no coffee beans in the database
                if (!_dbContext.CoffeeBean.Any())
                {
                    // Call the json service
                    await _jsonService.LoadDataFromFile("Data/json/coffeebeans.json");
                }

                // Return true or false
                return _dbContext.CoffeeBean.Any() ? true : false;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Return a list of coffee beans by ID
        /// </summary>
        /// <returns></returns>
        public async Task<List<CoffeeBean>> GetAll() {
            return await _dbContext.CoffeeBean.OrderBy(x => x.Id).ToListAsync();
        }

        /// <summary>
        /// Delete all coffee beans from the database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteAll()
        {
            try
            {
                // Execute raw command due to speed and efficiency 
                _dbContext.Database.ExecuteSqlRaw("DELETE FROM CoffeeBean");
                
                List<CoffeeBean> existingBeans = await _dbContext.CoffeeBean.ToListAsync();

                if (existingBeans.Count() == 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while trying to delete all coffee beans");
            }

            return false;
        }

        /// <summary>
        /// Insert a new coffee bean into the database
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> InsertCoffeeBean(InsertCoffeeBeanRequest request)
        {
            try
            {
                await _dbContext.CoffeeBean.AddAsync(new CoffeeBean() {
                    Name = request.Name,
                    Cost = request.Cost,
                    Description = request.Description,
                    Colour = request.Colour,
                    Image = CheckImage(request.Image)
                });

                var newRows = await _dbContext.SaveChangesAsync();

                // Return true or false based on whether data has been added
                return newRows > 0 ? true : false;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception while trying to insert coffee bean with name : {request.Name}");
                throw ex;
            }
        }

        /// <summary>
        /// Update Coffee Bean
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> UpdateCoffeeBean(UpdateCoffeeBeanRequest request)
        {
            try
            {
                // Get the bean and check it exists
                CoffeeBean existingBean = await _dbContext.CoffeeBean.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                if(existingBean == null)
                {
                    return false;
                }

                // Update values making sure not to update them if they a null or empty
                existingBean.Description = request.Description.IsNullOrEmpty() ? existingBean.Description : request.Description;
                existingBean.Colour = request.Colour.IsNullOrEmpty() ? existingBean.Colour : request.Colour;
                existingBean.Cost = request.Cost.IsNullOrEmpty() ? existingBean.Cost : request.Cost;
                existingBean.Name = request.Name.IsNullOrEmpty() ? existingBean.Name : request.Name;
                existingBean.Image  = CheckImage(request.Image).IsNullOrEmpty() ? existingBean.Image : CheckImage(request.Image);

                int affectedRows = await _dbContext.SaveChangesAsync();

                // Return true or false based on whether data has been added
                return affectedRows > 0 ? true : false;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception while trying to insert coffee bean with name : {request.Name}");
                throw ex;
            }
        }

        public async Task<List<CoffeeBean>> Search(SearchCoffeeBeansRequest request)
        {
            try
            {
                IQueryable<CoffeeBean> query = _dbContext.CoffeeBean;

                if (!string.IsNullOrEmpty(request.Name))
                {
                    query = query.Where(b => b.Name.Contains(request.Name));
                }

                if (!string.IsNullOrEmpty(request.Colour))
                {
                    query = query.Where(b => b.Colour.Contains(request.Colour));
                }


                if (!string.IsNullOrEmpty(request.Cost))
                {
                    query = query.Where(b => b.Cost.Contains(request.Cost));
                }

                List<CoffeeBean> result = await query.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception occurred while trying to search coffee beans for the filters: {request.Name} - {request.Cost} - {request.Colour}");
                throw ex;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// If the image is null, grab a placeholder
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private string CheckImage(string? image)
        {
            try
            {
                if (!image.IsNullOrEmpty())
                {
                    return image;
                }

                return "https://archive.org/download/placeholder-image/placeholder-image.jpg";
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception while trying to retrieve an image.");
                throw ex;
            }
        }

        #endregion Private Methods
    }
}
