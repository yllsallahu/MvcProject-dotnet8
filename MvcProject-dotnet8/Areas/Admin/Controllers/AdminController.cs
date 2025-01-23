using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcProject_dotnet8.Data;
using MvcProject.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyMVCProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Vet�m Admin ka akses
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// FAQJA KRYESORE E ADMIN (DASHBOARD)
        /// Shfaq disa statistika baz�: numrin e p�rdoruesve, produkteve, etj.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            // Shembull i nxjerrjes s� disa statistikave:
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalRoles = await _roleManager.Roles.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            // Mund t� shtoni edhe statistika t� tjera sipas nevoj�s.

            // Opsionale: logim veprimi
            await LogAction("Visit Admin Dashboard", null);

            return View();
        }

        // ------------------- MENAXHIMI I P�RDORUESVE -------------------

        /// <summary>
        /// LISTIMI I P�RDORUESVE
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ListUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            // Opsionale: logim veprimi
            await LogAction("List Users", null);

            return View(users);
        }

        /// <summary>
        /// DETAJE P�RDORUESI
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns></returns>
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Opsionale: lista e roleve q� ka ky user
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;

            // Logim
            await LogAction("View User Details", id);

            return View(user);
        }

        /// <summary>
        /// Fshirja e nj� p�rdoruesi nga sistemi
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // K�tu mund t� shtoni logjik� shtes�, p.sh. �far� ndodh me entitetet q� i takojn� k�tij user-i.

            await _userManager.DeleteAsync(user);

            // Logim
            await LogAction("Delete User", id);

            return RedirectToAction(nameof(ListUsers));
        }

        // ------------------- MENAXHIMI I ROLE-VE -------------------

        /// <summary>
        /// LISTA E ROLEVE
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ListRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            // Logim
            await LogAction("List Roles", null);
            return View(roles);
        }

        /// <summary>
        /// KRIJIMI I ROLEVE T� REJA
        /// </summary>
        /// <returns></returns>
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                    // Logim
                    await LogAction($"Create Role '{roleName}'", null);
                }
            }
            return RedirectToAction(nameof(ListRoles));
        }

        /// <summary>
        /// ASIGNIMI I NJ� ROLE TEK NJ� USER
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Shtoni user-in n� rolin e k�rkuar
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                // Logim
                await LogAction($"Add User '{user.Email}' to Role '{roleName}'", userId);
            }

            return RedirectToAction(nameof(UserDetails), new { id = userId });
        }

        /// <summary>
        /// HEQJA E ROLIT NGA USER
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                // Logim
                await LogAction($"Remove User '{user.Email}' from Role '{roleName}'", userId);
            }

            return RedirectToAction(nameof(UserDetails), new { id = userId });
        }

        // ------------------- MENAXHIMI I LEJEVE / ACCESS N� ENTITETE -------------------
        /// <summary>
        /// Shembull: Menaxhimi i cil�t p�rdorues mund t� shohin apo editojn� nj� entitet (p.sh. Product).
        /// K�tu thjesht ilustrojm� si mund ta realizoni, nuk �sht� i plot� pa nj� tabel� shtes� p.sh. "EntityPermissions".
        /// </summary>
        public async Task<IActionResult> ManagePermissions(int productId)
        {
            // Gjejm� produktin n� fjal�
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            // Lista e gjith� p�rdoruesve
            var users = await _userManager.Users.ToListAsync();

            // Mund t� keni nj� tabel� "EntityPermissions" q� lidhet me (ProductId, UserId, CanView, CanEdit, etj.)
            // K�tu vet�m ilustrojm� iden�.
            ViewBag.AllUsers = users;
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> ManagePermissions(int productId, string userId, bool canView, bool canEdit)
        {
            // Logjik� e brendshme: ruani ose p�rdit�soni n� DB se ky user ka t� drejt� ta shoh� apo editoj� product me Id=productId.
            // Shembull i thjesht�, do t�ju duhet nj� entitet i posa��m p.sh. "ProductPermission".
            // K�tu vet�m demonstrojm� sesi mund t� quhet ky aksion.

            // Logim
            await LogAction($"Set Permissions (canView={canView}, canEdit={canEdit}) for user={userId} on product={productId}", userId);

            return RedirectToAction(nameof(ManagePermissions), new { productId });
        }

        // ------------------- LOG VIEW -------------------
        /// <summary>
        /// Shfaq t� gjitha veprimet (CRUD) q� jan� loguar n� tabel�n AuditLogs
        /// </summary>
        public async Task<IActionResult> ViewLogs()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            // Asnj� logim k�tu, p�r t� shmangur loop (mund ta logoni, por do t� shtohet �do her�).
            return View(logs);
        }

        // ------------------- FUNKSION ND�RTORES P�R LOGIM (AUDIT) -------------------
        private async Task LogAction(string actionDescription, string userId)
        {
            // userId mund ta lini null n�se nuk ka user t� p�rfshir� ose p�rdorni at� aktual
            var currentUserId = userId ?? _userManager.GetUserId(User);

            var auditLog = new AuditLog
            {
                Action = actionDescription,
                UserId = currentUserId
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
