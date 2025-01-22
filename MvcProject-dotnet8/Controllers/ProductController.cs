using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMVCProject.Data;
using MyMVCProject.Models;
using System.Threading.Tasks;
using System.Linq;

namespace MyMVCProject.Controllers
{
	public class ProductController : Controller
	{
		private readonly ApplicationDbContext _context;

		// Konstruktor
		public ProductController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Product
		public async Task<IActionResult> Index(string searchName, string sortOrder, int page = 1)
		{
			int pageSize = 5; // Madhësia e çdo faqe

			// Marrja e produkteve nga databaza
			var products = from p in _context.Products
						   select p;

			// Filtrim (nëse shfaqet nga query)
			if (!string.IsNullOrEmpty(searchName))
			{
				products = products.Where(p => p.Name.Contains(searchName));
			}

			// Renditje
			products = (sortOrder == "name_desc")
				? products.OrderByDescending(p => p.Name)
				: products.OrderBy(p => p.Name);

			// Pagination
			int totalItems = await products.CountAsync();
			var items = await products
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Kalo variabla ndihmëse te ViewBag
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);
			ViewBag.SearchName = searchName;
			ViewBag.SortOrder = sortOrder;

			// Kthe pamjen me listën e produkteve
			return View(items);
		}

		// GET: Product/Details/5
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

		// GET: Product/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Product/Create
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

		// GET: Product/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return NotFound();

			var product = await _context.Products.FindAsync(id);

			if (product == null)
				return NotFound();

			return View(product);
		}

		// POST: Product/Edit/5
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

		// GET: Product/Delete/5
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

		// POST: Product/Delete/5
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
