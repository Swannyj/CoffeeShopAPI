using System.Text.RegularExpressions;
using Azure.Core;
using CoffeeShopAPI.Data;
using CoffeeShopAPI.Data.Interfaces;
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Models.RequestTypes;
using CoffeeShopAPI.Models.ResponseTypes;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoffeeBeanAPIController : ControllerBase
    {
        #region Global Variables

        private readonly AppDbContext _context;
        private readonly ICoffeeBeanRepository _coffeeBeanRepository;
        private readonly ILogger<CoffeeBeanAPIController> _logger;

        #endregion Global Variables

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="coffeeBeanRepository"></param>
        /// <param name="logger"></param>
        public CoffeeBeanAPIController(AppDbContext context, ICoffeeBeanRepository coffeeBeanRepository, ILogger<CoffeeBeanAPIController> logger)
        {
            _context = context;
            _coffeeBeanRepository = coffeeBeanRepository;
            _logger = logger;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Retrieve all coffee beans from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAsync")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var coffeeBeans = await _coffeeBeanRepository.GetAll();
                CoffeeBeanResponse response = new CoffeeBeanResponse();

                if (coffeeBeans.Count() > 0)
                {
                    response.coffeeBeans = coffeeBeans;
                }

                return Ok(response);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to get all coffee beans.");
               return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// DeleteAllCoffeeBeans
        /// </summary>
        /// <returns></returns>
        [HttpDelete("DeleteAllCoffeeBeansAsync")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<IActionResult> DeleteAllCoffeeBeansAsync()
        {
            try
            {
                bool success =  await _coffeeBeanRepository.DeleteAll();

                BaseResponse response = new BaseResponse();
                response.success = success;

                if (!success)
                {
                    response.message = "Unable to delete all coffee beans.";
                }

                return Ok(response);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Unhandled exception occurred while trying to delete all coffee beans.");
                return StatusCode(500, "An unexpected error occurred.");
            }

        }

        /// <summary>
        /// Load data from a json file
        /// </summary>
        /// <returns></returns>
        [HttpPost("LoadInitialDataAsync")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<IActionResult> LoadInitialDataAsync()
        {
            try
            {
                BaseResponse response = new BaseResponse();

                bool success = await _coffeeBeanRepository.LoadInitialData();

                response.success = success;
                if (!success)
                {
                    response.message = "Unable to load coffee bean data.";
                }

                return Ok(response);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Unhandled exception occurred while trying to load initial data.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Insert a new coffee bean into the database
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("InsertCoffeeBeanAsync")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<IActionResult> InsertCoffeeBeanAsync(InsertCoffeeBeanRequest request)
        {
            string pattern = @"^[^\d\s]{1}\d+(\.\d{2})$";

            // Check the cost value - ensure it is a string with a non-numeric at index 0
            if (!ModelState.IsValid || !Regex.IsMatch(request.Cost, pattern))
            {
                return BadRequest();
            }

            try
            {
                BaseResponse response = new BaseResponse();
                bool created = await _coffeeBeanRepository.InsertCoffeeBean(request);
                response.success = created;

                if (!created)
                {
                    response.message = "Insert failed";
                }

                return Ok(response);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while trying to add a new coffee bean.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Update an existing coffee bean
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("UpdateCoffeeBeanAsync")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<IActionResult> UpdateCoffeeBeanAsync(UpdateCoffeeBeanRequest request)
        {
            string pattern = @"^[^\d\s]{1}\d+(\.\d{2})$";

            // Check the cost value - ensure it is a string with a non-numeric at index 0
            if (!ModelState.IsValid || !Regex.IsMatch(request.Cost, pattern))
            {
                return BadRequest();
            }

            try
            {
                BaseResponse response = new BaseResponse();
                bool success = await _coffeeBeanRepository.UpdateCoffeeBean(request);
                response.success = success;

                if (!success)
                {
                    response.message = "Update failed";
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception occurred while trying to update coffee bean with id: {request.Id}.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// This function is used to test the functionality of the BOTD job
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        [HttpPost("Test-BOTD")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<IActionResult> TestBOTD([FromServices] BOTDService service)
        {
            try
            {
                await service.SelectBeanOfTheDayAsync(_context);

                var botd = await _context.CoffeeBean.FirstOrDefaultAsync(b => b.IsBOTD);
                return Ok(botd);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception occurred while testing BOTD.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Search coffee beans using name/ cost/ colour
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("search")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<IActionResult> Search([FromQuery] SearchCoffeeBeansRequest request)
        {
            try
            {
                List<CoffeeBean> result = await _coffeeBeanRepository.Search(request);

                return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception occurred while trying to search coffee beans for the filters: {request.Name} - {request.Cost} - {request.Colour}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        #endregion Public Methods
    }
}
