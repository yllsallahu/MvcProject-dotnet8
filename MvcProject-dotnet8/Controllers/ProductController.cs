using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcProject_dotnet8.Data;
using MvcProject.Models;
using System.Threading.Tasks;
using System.Linq;

namespace MvcProject_dotnet8.Controllers
{
	public class ProductController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ProductController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(string searchName, string sortOrder, int page = 1)
		{
			int pageSize = 5;

			var products = from p in _context.Products
						   select p;

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
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);
			ViewBag.SearchName = searchName;
			ViewBag.SortOrder = sortOrder;

			return View(items);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var product = await _context.Products
				.FirstOrDefaultAsync(m => m.Id == id);

			if (product == null)
				return NotFound();

			return View(product);
		}

        [Authorize(Roles = "Admin")]
        public IActionResult Create()

		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Price")] Product product)
		{
			if (ModelState.IsValid)
			{
				_context.Add(product);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(product);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return NotFound();

			var product = await _context.Products.FindAsync(id);

			if (product == null)
				return NotFound();

			return View(product);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price")] Product product)
		{
			if (id != product.Id)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(product);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ProductExists(product.Id))
						return NotFound();

					throw;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(product);
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			var product = await _context.Products
				.FirstOrDefaultAsync(m => m.Id == id);

			if (product == null)
				return NotFound();

			return View(product);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var product = await _context.Products.FindAsync(id);

			if (product != null)
			{
				_context.Products.Remove(product);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}

		private bool ProductExists(int id)
		{
			return _context.Products.Any(e => e.Id == id);
		}
	}
}
