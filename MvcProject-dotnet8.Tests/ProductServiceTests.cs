using Microsoft.EntityFrameworkCore;
using MvcProject.Models;
using MvcProject_dotnet8.Data;
using Xunit;

namespace MvcProject_dotnet8.Tests
{
    public class ProductServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

        public ProductServiceTests()
        {
            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CreateProduct_ValidatesPrice()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var product = new Product 
            { 
                Name = "Test Product", 
                Price = -10 // Invalid price
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => 
                context.Products.AddAsync(product));
        }

        [Fact]
        public async Task GetProducts_FiltersByCreator()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var userId = "testuser123";
            var products = new[]
            {
                new Product { Name = "Product 1", Price = 10, CreatedByUserId = userId },
                new Product { Name = "Product 2", Price = 20, CreatedByUserId = "other" }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Act
            var userProducts = await context.Products
                .Where(p => p.CreatedByUserId == userId)
                .ToListAsync();

            // Assert
            Assert.Single(userProducts);
            Assert.Equal("Product 1", userProducts[0].Name);
        }
    }
}
