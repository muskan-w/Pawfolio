using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using pawfolio_webapp.Data;
using pawfolio_webapp.Models;

namespace pawfolio_webapp.Areas.Identity.Pages.Account;

[Authorize(Roles = "VeterinaryProfessional")]
public class PendingApprovalModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public PendingApprovalModel(
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        var vetInfo = user == null
            ? null
            : await _context.VetInfos.FindAsync(user.Id);

        if (vetInfo?.ApprovalStatus == "Pending")
        {
            return Page();
        }

        if (vetInfo?.ApprovalStatus == "Approved")
        {
            return RedirectToAction("Index", "VeterinaryProfessionals");
        }

        return Forbid();
    }
}
