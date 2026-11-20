// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using System.ComponentModel.DataAnnotations;
using System.Text;

namespace pawfolio_webapp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        private readonly IConfiguration _configuration;

        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly HashSet<string> AllowedRoles =
            new(StringComparer.Ordinal)
            {
        "PetOwner",
        "VeterinaryProfessional"
            };
        public RegisterModel(
             UserManager<IdentityUser> userManager,
             IUserStore<IdentityUser> userStore,
             SignInManager<IdentityUser> signInManager,
             ILogger<RegisterModel> logger,
             IConfiguration configuration,
             IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Account Type")]
            public string Role { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!AllowedRoles.Contains(Input.Role))
            {
                ModelState.AddModelError(
                    nameof(Input.Role),
                    "The selected account type is not valid.");

                return Page();
            }

            var user = CreateUser();

            await _userStore.SetUserNameAsync(
                user,
                Input.Email,
                CancellationToken.None);

            await _emailStore.SetEmailAsync(
                user,
                Input.Email,
                CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Input.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedConfirmationCode = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(confirmationCode));
            var confirmationLink = Url.PageLink(
                "/Account/ConfirmEmail",
                values: new { area = "Identity", userId = user.Id, code = encodedConfirmationCode },
                protocol: Request.Scheme);

            try
            {
                var apiKey = _configuration["Smtp2Go:ApiKey"]?.Trim();
                var senderEmail = _configuration["Smtp2Go:SenderEmail"]?.Trim();
                var recipientEmail = Input.Email?.Trim();

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new InvalidOperationException(
                        "Smtp2Go:ApiKey is missing.");
                }

                if (string.IsNullOrWhiteSpace(senderEmail))
                {
                    throw new InvalidOperationException(
                        "Smtp2Go:SenderEmail is missing.");
                }

                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    throw new InvalidOperationException(
                        "The recipient email address is missing.");
                }

                var payload = new
                {
                    api_key = apiKey,
                    sender = senderEmail,
                    to = new[] { recipientEmail },
                    subject = "Confirm your Pawfolio account",
                    text_body = $"Welcome to Pawfolio, {Input.FirstName}! Confirm your email: {confirmationLink}",
                    html_body =
                        $"<html><body>" +
                        $"<h1>Welcome to Pawfolio, {Input.FirstName}!</h1>" +
                        $"<p>Your account has been created. <a href=\"{confirmationLink}\">Confirm your email address</a> to finish registration.</p>" +
                        "</body></html>"
                };

                var httpClient = _httpClientFactory.CreateClient("Smtp2Go");

                using var httpResponse = await httpClient.PostAsJsonAsync(
                    "v3/email/send",
                    payload);

                var responseBody =
                    await httpResponse.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "SMTP2GO HTTP status: {StatusCode}; response: {Response}",
                    (int)httpResponse.StatusCode,
                    responseBody);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"SMTP2GO rejected the request: {responseBody}");
                }

                _logger.LogInformation(
                    "User created a new account with password.");

                return RedirectToPage(
                    "RegisterConfirmation",
                    new
                    {
                        email = Input.Email,
                        returnUrl
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SMTP2GO email delivery failed for {Email}.",
                    Input.Email);

                // Do not leave an account that cannot complete registration.
                var deleteResult = await _userManager.DeleteAsync(user);

                if (!deleteResult.Succeeded)
                {
                    _logger.LogError(
                        "The user {Email} could not be deleted after email failure.",
                        Input.Email);
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Registration could not be completed because the confirmation " +
                    "email could not be sent.");

                return Page();
            }
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
