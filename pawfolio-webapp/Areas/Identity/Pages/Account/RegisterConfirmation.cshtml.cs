using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Smtp2Go.Api;
using Smtp2Go.Api.Models.Emails;
using System.Security.Cryptography;

namespace pawfolio_webapp.Areas.Identity.Pages.Account;

public class RegisterConfirmationModel() : PageModel
{
    public string? Email { get; private set; }

    //API key - delete when done testing
    private readonly Smtp2GoApiService service =
        new Smtp2GoApiService("api-27B5646477944E2C896A48AB26CC5BB0");


    public IActionResult OnGet(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("Register");
        }

        Email = email;

        var message = new EmailMessage(
            "From Pawfolio <info@pawfolio.com>",
            email);

        //email message content
        message.Subject = "Welcome to Pawfolio!";
        message.BodyHtml = "<html><body>" +
            "<h1>Welcome to Pawfolio</h1>" +
            "<p>Your email is now registered and you are free to <a href='https://pawfolio.runasp.net/Account/Login'>login</a>.</p>" +
            "</body></html>";
        message.BodyText = "Welcome to Pawfolio! Your email is now registered and you are free to login";

        var result = service.SendEmail(message);

        return Page();
    }
}
