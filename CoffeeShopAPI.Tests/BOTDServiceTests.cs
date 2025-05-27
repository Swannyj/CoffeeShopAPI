using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShopAPI.Data;
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoffeeShopAPI.Tests
{
    public class BOTDServiceTests
    {
        #region Global Variables

        private readonly BOTDService _botdService;

        #endregion Global Variables

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BOTDServiceTests()
        {
            var loggerMock = new Mock<ILogger<BOTDService>>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _botdService = new BOTDService(scopeFactoryMock.Object, loggerMock.Object);
        }

        #endregion Constructir

        #region Tests

        /// <summary>
        /// Set one bean as BOTD
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SelectBeanOfTheDayAsync_SetsOneBeanAsBOTD()
        {
            // set up in-memory DB
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            // Seed with test beans
            context.CoffeeBean.AddRange(await GetCoffeeBeans());
            await context.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<BOTDService>>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            var service = new BOTDService(scopeFactoryMock.Object, loggerMock.Object);

            // Act
            await service.SelectBeanOfTheDayAsync(context);

            // Assert
            var beans = context.CoffeeBean.ToList();
            var botdCount = beans.Count(b => b.IsBOTD);
            Assert.Equal(1, botdCount); // Only one bean should be BOTD
        }

        /// <summary>
        /// If there are no other beans. Do nothing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SelectBeanOfTheDayAsync_NoOtherBeans_DoesNothing()
        {
            // set up in-memory DB
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            // Seed with all beans already BOTD
            context.CoffeeBean.Add(new CoffeeBean
            {
                Id = 2,
                Name = "Robusta",
                IsBOTD = true,
                Colour = "Dark Brown",
                Cost = "£10.00",
                Description = "Strong coffee bean"
            });

            await context.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<BOTDService>>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            var service = new BOTDService(scopeFactoryMock.Object, loggerMock.Object);

            // Act
            await service.SelectBeanOfTheDayAsync(context);

            // Assert: No changes should have occurred
            var bean = context.CoffeeBean.First();
            Assert.True(bean.IsBOTD);
        }

        /// <summary>
        /// Attempt to test what the affect Universal Time may have
        /// </summary>
        /// <param name="utcTimeString"></param>
        /// <param name="expectedHours"></param>
        [Theory]
        [InlineData("2025-03-09T23:30:00Z", 0.5)]  // 23:30 UTC
        [InlineData("2025-03-10T00:30:00Z", 23.5)] // 00:30 UTC next day
        [InlineData("2025-11-01T23:30:00Z", 0.5)]  // 23:30 UTC again (no DST in UTC)
        [InlineData("2025-11-02T00:30:00Z", 23.5)] // 00:30 UTC
        public void CalculateDelayUntilNextMidnight_UTC_NoDST(string utcTimeString, double expectedHours)
        {
            DateTime utcTime = DateTime.Parse(utcTimeString).ToUniversalTime();

            TimeSpan delay = _botdService.CalculateDelayUntilNextMidnight(utcTime);

            // Assert
            Assert.InRange(delay.TotalHours, expectedHours - 0.001, expectedHours + 0.001);
        }

        #endregion Tests

        #region Private Methods

        /// <summary>
        /// Create some coffee beans
        /// </summary>
        /// <returns></returns>
        public async Task<List<CoffeeBean>> GetCoffeeBeans()
        {
            return new List<CoffeeBean>() {
                new CoffeeBean
                {
                    Id = 1,
                    Name = "Arabica",
                    IsBOTD = false,
                    Colour = "Brown",
                    Cost = "$12.99",
                    Description = "Smooth coffee bean"
                },
                new CoffeeBean
                {
                    Id = 2,
                    Name = "Robusta",
                    IsBOTD = false,
                    Colour = "Dark Brown",
                    Cost = "£10.00",
                    Description = "Strong coffee bean"
                },
                 new CoffeeBean
                {
                    Id = 3,
                    Name = "Liberica",
                    IsBOTD = false,
                    Colour = "Light Brown",
                    Cost = "£5.99",
                    Description = "Unique flavor coffee bean"
                }
            };
        }

        #endregion Private Methods
    }
}
