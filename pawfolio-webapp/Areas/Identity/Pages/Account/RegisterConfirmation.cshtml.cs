using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Smtp2Go.Api;
using Smtp2Go.Api.Models.Emails;
Model Email;


namespace pawfolio_webapp.Areas.Identity.Pages.Account;

public class RegisterConfirmationModel(IWebHostEnvironment environment) : PageModel
{
    public string? Email { get; private set; }

    private readonly Smtp2GoApiService service =
        new Smtp2GoApiService("api-27B5646477944E2C896A48AB26CC5BB0");

    public bool IsDevelopment => environment.IsDevelopment();

    public IActionResult OnGet(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("Register");
        }

        Email = email;

        var message = new EmailMessage("From Pawfolio <from@pawfolio.com>", "To" + @Model.Email);

        message.BodyHtml = "<html>Email HTML Body<html/>";
        message.BodyText = "Email Plain Text Body";

        var result = service.SendEmail(message);
        return Page();
    }
}
