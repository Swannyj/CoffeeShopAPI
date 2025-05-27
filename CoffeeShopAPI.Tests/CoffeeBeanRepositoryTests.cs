using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShopAPI.Data.Interfaces;
using CoffeeShopAPI.Data.Repositories;
using CoffeeShopAPI.Data;
using CoffeeShopAPI.Models.RequestTypes;
using CoffeeShopAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoffeeShopAPI.Tests
{
    #region Public Methods

    /// <summary>
    /// CoffeeBeanRepository Tests
    /// </summary>
    public class CoffeeBeanRepositoryTests
    {

        /// <summary>
        /// Test to see if the initial data can load if there are no beans
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LoadInitialData_WhenNoBeans_CallsJsonServiceLoadData()
        {
            var context = CreateDbContext();
            var jsonServiceMock = new Mock<IJSONService>();
            jsonServiceMock.Setup(j => j.LoadDataFromFile(It.IsAny<string>())).Returns(Task.CompletedTask);

            var repo = CreateRepository(context, jsonServiceMock);

            // load data
            var result = await repo.LoadInitialData();

            // Assert
            jsonServiceMock.Verify(j => j.LoadDataFromFile("Data/json/coffeebeans.json"), Times.Once);
            Assert.False(result); // Because we never added any beans after LoadDataFromFile call in this test
        }

        /// <summary>
        /// Return all beans ordered by Id
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAll_ReturnsAllBeansOrderedById()
        {
            var context = CreateDbContext();
            context.CoffeeBean.AddRange(await GetCoffeeBeans());

            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            var result = await repo.GetAll();

            // Check the correct count
            Assert.Equal(3, result.Count);

            // Ensure they are returned by ID
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
            Assert.Equal(3, result[2].Id);
        }

        /// <summary>
        /// Insert coffee bean and ensure it has saved
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertCoffeeBean_AddsBean_ReturnsTrue()
        {
            var context = CreateDbContext();
            var repo = CreateRepository(context);

            var request = new InsertCoffeeBeanRequest
            {
                Name = "Dev Bean",
                Cost = "$8.99",
                Description = "This is a test bean used for tests",
                Colour = "Brown",
                Image = null
            };

            // Call the repo
            var result = await repo.InsertCoffeeBean(request);

            Assert.True(result);

            // Try and get it
            var bean = await context.CoffeeBean.FirstOrDefaultAsync();

            Assert.NotNull(bean);
            Assert.Equal("Dev Bean", bean.Name);

            // As no image was added, the created bean should have the placeholder image
            Assert.Equal("https://archive.org/download/placeholder-image/placeholder-image.jpg", bean.Image);
        }

        /// <summary>
        /// Update a bean if it exists
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateCoffeeBean_UpdatesExistingBean_ReturnsTrue()
        {
            var context = CreateDbContext();
            var bean = new CoffeeBean
            {
                Id = 1,
                Name = "Old Name",
                Cost = "5",
                Description = "Old Desc",
                Colour = "Green",
                Image = "old.png"
            };
            context.CoffeeBean.Add(bean);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            var request = new UpdateCoffeeBeanRequest
            {
                Id = 1,
                Name = "New Name",
                Cost = "10",
                Description = "New Desc",
                Colour = "Brown",
                Image = "new.png"
            };

            var result = await repo.UpdateCoffeeBean(request);

            Assert.True(result);

            var updatedBean = await context.CoffeeBean.FirstOrDefaultAsync(b => b.Id == 1);
            Assert.NotNull(updatedBean);

            // Check to ensure that the values match what we've updated
            Assert.Equal("New Name", updatedBean.Name);
            Assert.Equal("10", updatedBean.Cost);
            Assert.Equal("New Desc", updatedBean.Description);
            Assert.Equal("Brown", updatedBean.Colour);
            Assert.Equal("new.png", updatedBean.Image);
        }

        /// <summary>
        /// Try and update a bean that cannot be found
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateCoffeeBean_ReturnsFalse_WhenBeanNotFound()
        {
            var context = CreateDbContext();
            var repo = CreateRepository(context);

            var request = new UpdateCoffeeBeanRequest
            {
                Id = 99,
                Name = "New Name"
            };

            var result = await repo.UpdateCoffeeBean(request);

            Assert.False(result);
        }

        /// <summary>
        /// Test the search functionality
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Search_FiltersByNameColourCost_ReturnsFilteredList()
        {
            var context = CreateDbContext();
            context.CoffeeBean.AddRange(await GetCoffeeBeans());
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            var request = new SearchCoffeeBeansRequest
            {
                Name = "TU",
                Colour = "dar",
                Cost = "3"
            };

            var results = await repo.Search(request);

            Assert.Single(results);
            Assert.Equal("TURNABOUT", results[0].Name);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Create the DB
        /// </summary>
        /// <returns></returns>
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())  // Unique DB per test
                .Options;

            return new AppDbContext(options);
        }

        /// <summary>
        /// Create Repository
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jsonServiceMock"></param>
        /// <param name="loggerMock"></param>
        /// <returns></returns>
        private CoffeeBeanRepository CreateRepository(AppDbContext context, Mock<IJSONService>? jsonServiceMock = null, Mock<ILogger<CoffeeBeanRepository>>? loggerMock = null)
        {
            jsonServiceMock ??= new Mock<IJSONService>();
            loggerMock ??= new Mock<ILogger<CoffeeBeanRepository>>();
            return new CoffeeBeanRepository(context, jsonServiceMock.Object, loggerMock.Object);
        }

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
                    Name = "TURNABOUT",
                    IsBOTD = false,
                    Colour = "dark roast",
                    Cost = "£39.26",
                    Description = "Smooth coffee bean"
                },
                new CoffeeBean
                {
                    Id = 2,
                    Name = "ISONUS",
                    IsBOTD = false,
                    Colour = "Dark Brown",
                    Cost = "£10.00",
                    Description = "Strong coffee bean"
                },
                 new CoffeeBean
                {
                    Id = 3,
                    Name = "ZILLAN",
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
