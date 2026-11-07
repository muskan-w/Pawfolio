using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pawfolio_webapp.Data;
using pawfolio_webapp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace pawfolio_webapp.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if the user is logged in
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return View();
            }

            // Fetch roles from db
            var userRoles = _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => _context.Roles.FirstOrDefault(r => r.Id == ur.RoleId).Name)
            .ToList();

            // Redirect based on the roles
            if (userRoles.Contains("PetOwner"))
            {
                return RedirectToAction("Index", "PetOwners");
            }
            if (userRoles.Contains("VeterinaryProfessional"))
            {
                return RedirectToAction("Index", "VeterinaryProfessionals");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
