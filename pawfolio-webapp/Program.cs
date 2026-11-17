using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using pawfolio_webapp.Data;
using pawfolio_webapp.Models;
using Smtp2Go;
using Smtp2Go.Api;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure(//retry logic
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() //adding role support
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Configure the cookie registered by AddDefaultIdentity. Identity pages are in
// the Identity area, so the paths must include the area route prefix.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Authentication and authorization policies.
var configuration = builder.Configuration;
// Add Google Authentication services --
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
});
//session variable to store user role
builder.Services.AddSession();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


    foreach (var roleName in new[] {"PetOwner", "VeterinaryProfessional" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create the {roleName} role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

//using session variable to store user role
app.UseSession();

app.Run();
