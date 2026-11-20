using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace pawfolio_webapp.Areas.Identity.Pages.Account;

public class RegisterConfirmationModel : PageModel
{
    public string? Email { get; private set; }

    public IActionResult OnGet(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("Register");
        }

        Email = email;
        return Page();
    }
}
