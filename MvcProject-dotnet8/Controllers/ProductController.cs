using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcProject_dotnet8.Data;
using MvcProject.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace MvcProject_dotnet8.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchName, string sortOrder, int page = 1)
        {
            int pageSize = 5;

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var products = _context.Products
                .Where(p => isAdmin || 
                           p.CreatedByUserId == currentUserId ||
                           p.Permissions.Any(pp => pp.UserId == currentUserId && pp.CanView));

            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.Name.Contains(searchName));
            }

            products = (sortOrder == "name_desc")
                ? products.OrderByDescending(p => p.Name)
                : products.OrderBy(p => p.Name);

            int totalItems = await products.CountAsync();
            var items = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchName = searchName;
            ViewBag.SortOrder = sortOrder;

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Check permissions
            if (!await CanViewProduct(product))
            {
                return Forbid();
            }

            ViewBag.OrderCount = 1; // Default order count
            return View(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Helper methods for permission checking
        private async Task<bool> CanViewProduct(Product product)
        {
            if (User.IsInRole("Admin")) return true;

            var currentUserId = _userManager.GetUserId(User);
            
            return product.CreatedByUserId == currentUserId ||
                   await _context.ProductPermissions
                       .AnyAsync(pp => pp.ProductId == product.Id && 
                                     pp.UserId == currentUserId && 
                                     pp.CanView);
        }
    }
}
