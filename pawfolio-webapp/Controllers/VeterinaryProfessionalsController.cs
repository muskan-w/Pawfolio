using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using pawfolio_webapp.Data;
using pawfolio_webapp.Models;

namespace pawfolio_webapp.Controllers
{
    [Authorize(Roles = "VeterinaryProfessional")]
    public class VeterinaryProfessionalsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public VeterinaryProfessionalsController(
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var vetInfo = user == null
                ? null
                : await _context.VetInfos.FindAsync(user.Id);

            if (vetInfo?.ApprovalStatus != "Approved")
            {
                return RedirectToPage("/Account/PendingApproval", new { area = "Identity" });
            }

            return View();
        }
    }
}
