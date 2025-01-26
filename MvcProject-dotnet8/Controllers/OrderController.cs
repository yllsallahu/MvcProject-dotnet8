using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcProject.Models;
using MvcProject_dotnet8.Data;

namespace MvcProject_dotnet8.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)  // Make sure this is included
                .Where(o => o.IsActive)  // Only show active orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Ensure user data is loaded
            foreach (var order in orders)
            {
                if (order.User == null)
                {
                    order.User = await _userManager.FindByIdAsync(order.UserId);
                }
            }

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int productId, int quantity)
        {
            if (quantity <= 0 || quantity > 100)
            {
                TempData["Error"] = "Please enter a valid quantity between 1 and 100";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            var order = new Order
            {
                ProductId = productId,
                UserId = _userManager.GetUserId(User),
                Quantity = quantity,
                TotalPrice = product.Price * quantity,
                OrderDate = DateTime.UtcNow,
                Status = Order.StatusPending,
                IsActive = true
            };

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Order placed successfully!";
                return RedirectToAction(nameof(MyOrders));
            }
            catch (Exception)
            {
                TempData["Error"] = "There was an error placing your order. Please try again.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Cancelled";
            order.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order cancelled successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReceived(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != Order.StatusPending)
            {
                return BadRequest("Can only confirm receipt of pending orders.");
            }

            order.Status = Order.StatusReceived;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order marked as received!";
            return RedirectToAction(nameof(MyOrders));
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
