using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace pawfolio_webapp.Areas.Identity.Pages.Account;

public class ConfirmEmailModel(UserManager<IdentityUser> userManager) : PageModel
{
    public bool Confirmed { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            ErrorMessage = "The email confirmation link is invalid.";
            return Page();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            ErrorMessage = "Unable to find the account for this confirmation link.";
            return Page();
        }

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            ErrorMessage = "The email confirmation link is invalid.";
            return Page();
        }

        var result = await userManager.ConfirmEmailAsync(user, code);
        Confirmed = result.Succeeded;
        ErrorMessage = result.Succeeded
            ? null
            : string.Join(" ", result.Errors.Select(error => error.Description));

        return Page();
    }
}
