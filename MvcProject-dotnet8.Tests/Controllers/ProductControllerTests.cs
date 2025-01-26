using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MvcProject.Models;
using MvcProject_dotnet8.Controllers;
using MvcProject_dotnet8.Data;
using Xunit;

namespace MvcProject_dotnet8.Tests.Controllers
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
        public async Task Index_ReturnsAllProducts()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new ProductController(context, _mockUserManager.Object);
            var testProduct = new Product { Name = "Test Product", Price = 10.00m };
            context.Products.Add(testProduct);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Index(null, null) as ViewResult;
            var model = result?.Model as IEnumerable<Product>;

            // Assert
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Test Product", model.First().Name);
        }

        [Fact]
        public async Task Create_SavesProductWithCurrentUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var userId = "testuser123";
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns(userId);
            var controller = new ProductController(context, _mockUserManager.Object);
            var product = new Product { Name = "New Product", Price = 20.00m };

            // Act
            var result = await controller.Create(product) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(ProductController.Index), result.ActionName);
            var savedProduct = await context.Products.FirstOrDefaultAsync();
            Assert.Equal(userId, savedProduct.CreatedByUserId);
        }

        [Fact]
        public async Task Details_ReturnsNotFoundForInvalidId()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var controller = new ProductController(context, _mockUserManager.Object);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
