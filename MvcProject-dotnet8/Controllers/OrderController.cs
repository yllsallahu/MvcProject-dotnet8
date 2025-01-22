using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using MvcProject_dotnet8.Data;
using System;

namespace MvcProject_dotnet8.Controllers
{
    public class OrderController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IActionResult> Index(string searchName, string sortOrder, int page = 1)
        {
            int pageSize = 5; // caktimi i madhësisë së faqes

            var products = from p in _context.Products
                           select p;

            // Filtrim
            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.Name.Contains(searchName));
            }

            // Renditje
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            // Pagination
            int totalItems = await products.CountAsync();
            var items = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewBag ose ViewModel për të kaluar të dhënat
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchName = searchName;
            ViewBag.SortOrder = sortOrder;

            return View(items);
        }
    }
}
