using Microsoft.EntityFrameworkCore;
using RedisDotNet7API.Models;

namespace RedisDotNet7API
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }


        public DbSet<ProductModel> Products { get; set; }

    }
}
