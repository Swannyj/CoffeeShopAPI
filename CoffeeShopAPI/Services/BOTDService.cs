using CoffeeShopAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopAPI.Services
{
    /// <summary>
    /// Bean of the Day Service
    /// </summary>
    public class BOTDService : BackgroundService
    {
        #region Global Variables

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BOTDService> _logger;

        #endregion Global Variables

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scopeFactory"></param>
        /// <param name="logger"></param>
        public BOTDService(IServiceScopeFactory scopeFactory, ILogger<BOTDService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Execute Async
        /// Execute the background service at midnight every night
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    // attempt to select a bean
                    await SelectBeanOfTheDayAsync(dbContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to select Bean of the Day.");
                }

                // Calculate delay until next midnight
                var now = DateTime.Now;
                var nextMidnight = now.Date.AddDays(1); // midnight of next day
                var delay = nextMidnight - now;

                _logger.LogInformation($"Waiting {delay} until next execution at midnight.");

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // The delay was cancelled
                    break;
                }
            }
        }

        /// <summary>
        /// Select the bean of the day
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public async Task SelectBeanOfTheDayAsync(AppDbContext dbContext)
        {   
            // Get the current BOTD
            var currentBotd = await dbContext.CoffeeBean.FirstOrDefaultAsync(b => b.IsBOTD);

            // Get eligible beans
            var otherBeans = await dbContext.CoffeeBean.Where(x => !x.IsBOTD).ToListAsync();

            // If there are no eligible beans
            if (otherBeans.Count == 0)
                return;

            // Reset all to false
            foreach (var bean in dbContext.CoffeeBean)
            {
                bean.IsBOTD = false;
            }

            // Pick a random bean
            var random = new Random();
            var randomBean = otherBeans[random.Next(otherBeans.Count)];
            randomBean.IsBOTD = true;

            if (currentBotd != null)
            {
                // Set current to false
                currentBotd.IsBOTD = false;
            }

            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Bean of the Day: {randomBean.Name}");
        }

        #endregion Public Methods
    }

}
