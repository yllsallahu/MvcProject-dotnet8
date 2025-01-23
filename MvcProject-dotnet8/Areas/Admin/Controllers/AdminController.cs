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
    [Authorize(Roles = "Admin")] // Vetëm Admin ka akses
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
        /// Shfaq disa statistika bazë: numrin e përdoruesve, produkteve, etj.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            // Shembull i nxjerrjes së disa statistikave:
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalRoles = await _roleManager.Roles.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            // Mund të shtoni edhe statistika të tjera sipas nevojës.

            // Opsionale: logim veprimi
            await LogAction("Visit Admin Dashboard", null);

            return View();
        }

        // ------------------- MENAXHIMI I PËRDORUESVE -------------------

        /// <summary>
        /// LISTIMI I PËRDORUESVE
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
        /// DETAJE PËRDORUESI
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns></returns>
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Opsionale: lista e roleve që ka ky user
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;

            // Logim
            await LogAction("View User Details", id);

            return View(user);
        }

        /// <summary>
        /// Fshirja e një përdoruesi nga sistemi
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Këtu mund të shtoni logjikë shtesë, p.sh. çfarë ndodh me entitetet që i takojnë këtij user-i.

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
        /// KRIJIMI I ROLEVE TË REJA
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
        /// ASIGNIMI I NJË ROLE TEK NJË USER
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Shtoni user-in në rolin e kërkuar
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

        // ------------------- MENAXHIMI I LEJEVE / ACCESS NË ENTITETE -------------------
        /// <summary>
        /// Shembull: Menaxhimi i cilët përdorues mund të shohin apo editojnë një entitet (p.sh. Product).
        /// Këtu thjesht ilustrojmë si mund ta realizoni, nuk është i plotë pa një tabelë shtesë p.sh. "EntityPermissions".
        /// </summary>
        public async Task<IActionResult> ManagePermissions(int productId)
        {
            // Gjejmë produktin në fjalë
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            // Lista e gjithë përdoruesve
            var users = await _userManager.Users.ToListAsync();

            // Mund të keni një tabelë "EntityPermissions" që lidhet me (ProductId, UserId, CanView, CanEdit, etj.)
            // Këtu vetëm ilustrojmë idenë.
            ViewBag.AllUsers = users;
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> ManagePermissions(int productId, string userId, bool canView, bool canEdit)
        {
            // Logjikë e brendshme: ruani ose përditësoni në DB se ky user ka të drejtë ta shohë apo editojë product me Id=productId.
            // Shembull i thjeshtë, do t’ju duhet një entitet i posaçëm p.sh. "ProductPermission".
            // Këtu vetëm demonstrojmë sesi mund të quhet ky aksion.

            // Logim
            await LogAction($"Set Permissions (canView={canView}, canEdit={canEdit}) for user={userId} on product={productId}", userId);

            return RedirectToAction(nameof(ManagePermissions), new { productId });
        }

        // ------------------- LOG VIEW -------------------
        /// <summary>
        /// Shfaq të gjitha veprimet (CRUD) që janë loguar në tabelën AuditLogs
        /// </summary>
        public async Task<IActionResult> ViewLogs()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            // Asnjë logim këtu, për të shmangur loop (mund ta logoni, por do të shtohet çdo herë).
            return View(logs);
        }

        // ------------------- FUNKSION NDËRTORES PËR LOGIM (AUDIT) -------------------
        private async Task LogAction(string actionDescription, string userId)
        {
            // userId mund ta lini null nëse nuk ka user të përfshirë ose përdorni atë aktual
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
