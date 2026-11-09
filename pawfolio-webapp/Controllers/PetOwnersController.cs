using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pawfolio_webapp.Data;

namespace pawfolio_webapp.Controllers
{
    [Authorize]
    public class PetOwnersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PetOwnersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pets = _context.Pets
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Name)
                .ToList();

            return View(pets);
        }
    }
}
