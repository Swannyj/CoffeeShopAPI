using Microsoft.EntityFrameworkCore;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CoffeeBean> CoffeeBean { get; set; }
    }
}
