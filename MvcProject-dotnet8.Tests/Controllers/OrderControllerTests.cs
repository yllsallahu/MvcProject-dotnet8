public class OrderControllerTests
{
    private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

    public OrderControllerTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString())
            .Options;

        var userStore = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task PlaceOrder_CreatesNewOrder()
    {
        // Arrange
        using var context = new ApplicationDbContext(_contextOptions);
        var userId = "testuser123";
        _mockUserManager.Setup(x => x.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .Returns(userId);
        
        var product = new Product { Name = "Test Product", Price = 10.00m };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = new OrderController(context, _mockUserManager.Object);

        // Act
        var result = await controller.PlaceOrder(product.Id, 2) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nameof(OrderController.MyOrders), result.ActionName);
        var order = await context.Orders.FirstOrDefaultAsync();
        Assert.NotNull(order);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(2, order.Quantity);
        Assert.Equal(20.00m, order.TotalPrice);
    }
}
