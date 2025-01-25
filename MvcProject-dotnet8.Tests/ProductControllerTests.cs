using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MvcProject.Models;
using MvcProject_dotnet8.Controllers;
using MvcProject_dotnet8.Data;
using Xunit;

namespace MvcProject_dotnet8.Tests
{
    public class ProductControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

        public ProductControllerTests()
        {
            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString())
                .Options;

            var userStore = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Create_SetsCreatedByUserIdAndSavesToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var userId = "testuser123";
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns(userId);

            var controller = new ProductController(context, _mockUserManager.Object);
            var product = new Product { Name = "Test Product", Price = 99.99m };

            // Act
            var result = await controller.Create(product) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var savedProduct = await context.Products.FirstOrDefaultAsync(p => p.Name == "Test Product");
            Assert.NotNull(savedProduct);
            Assert.Equal(userId, savedProduct.CreatedByUserId);
        }

        [Fact]
        public async Task Edit_OnlyAllowsCreatorToEdit()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var userId = "testuser123";
            var product = new Product 
            { 
                Name = "Test Product", 
                Price = 99.99m, 
                CreatedByUserId = userId 
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("differentuser");

            var controller = new ProductController(context, _mockUserManager.Object);

            // Act
            var result = await controller.Edit(product.Id) as ForbidResult;

            // Assert
            Assert.NotNull(result);
        }
    }
}
