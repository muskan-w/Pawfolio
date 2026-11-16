using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pawfolio_webapp.Controllers;
using pawfolio_webapp.Data;

namespace pawfolio_webapp.Tests.Controllers;

[TestClass]
public class HomeControllerTests
{
    [TestMethod]
    public void Index_WhenUserIsUnauthenticated_ReturnsView()
    {
        using var context = CreateContext();
        var controller = CreateController(
            context,
            new ClaimsPrincipal(new ClaimsIdentity()));

        var result = controller.Index();

        Assert.IsInstanceOfType<ViewResult>(result);
    }

    [TestMethod]
    public void Index_WhenAuthenticatedWithoutUserId_ReturnsView()
    {
        using var context = CreateContext();
        var identity = new ClaimsIdentity("TestAuth");
        var controller = CreateController(
            context,
            new ClaimsPrincipal(identity));

        var result = controller.Index();

        Assert.IsInstanceOfType<ViewResult>(result);
    }

    [TestMethod]
    public void Index_WhenUserHasPetOwnerRole_RedirectsToPetOwners()
    {
        using var context = CreateContext();
        const string userId = "pet-owner-id";

        SeedRole(context, userId, "PetOwner");
        var controller = CreateController(context, AuthenticatedUser(userId));

        var result = controller.Index();

        var redirect = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.AreEqual("Index", redirect.ActionName);
        Assert.AreEqual("PetOwners", redirect.ControllerName);
    }

    [TestMethod]
    public void Index_WhenUserHasVeterinaryProfessionalRole_RedirectsToVeterinaryProfessionals()
    {
        using var context = CreateContext();
        const string userId = "veterinary-professional-id";

        SeedRole(context, userId, "VeterinaryProfessional");
        var controller = CreateController(context, AuthenticatedUser(userId));

        var result = controller.Index();

        var redirect = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.AreEqual("Index", redirect.ActionName);
        Assert.AreEqual("VeterinaryProfessionals", redirect.ControllerName);
    }

    [TestMethod]
    public void Index_WhenUserHasNoSupportedRole_ReturnsView()
    {
        using var context = CreateContext();
        const string userId = "unassigned-user-id";

        SeedRole(context, userId, "Customer");
        var controller = CreateController(context, AuthenticatedUser(userId));

        var result = controller.Index();

        Assert.IsInstanceOfType<ViewResult>(result);
    }

    [TestMethod]
    public void Privacy_ReturnsView()
    {
        using var context = CreateContext();
        var controller = CreateController(
            context,
            new ClaimsPrincipal(new ClaimsIdentity()));

        var result = controller.Privacy();

        Assert.IsInstanceOfType<ViewResult>(result);
    }

    [TestMethod]
    public void Error_ReturnsViewWithRequestId()
    {
        using var context = CreateContext();
        var controller = CreateController(
            context,
            new ClaimsPrincipal(new ClaimsIdentity()));

        controller.ControllerContext.HttpContext.TraceIdentifier = "request-123";

        var result = controller.Error();

        var view = Assert.IsInstanceOfType<ViewResult>(result);
        var model = Assert.IsInstanceOfType<pawfolio_webapp.Models.ErrorViewModel>(
            view.Model);

        Assert.AreEqual("request-123", model.RequestId);
    }

    private static HomeController CreateController(
        ApplicationDbContext context,
        ClaimsPrincipal user)
    {
        var controller = new HomeController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            }
        };

        return controller;
    }

    private static ClaimsPrincipal AuthenticatedUser(string userId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
            authenticationType: "TestAuth");

        return new ClaimsPrincipal(identity);
    }

    private static void SeedRole(ApplicationDbContext context, string userId, string roleName)
    {
        var role = new IdentityRole(roleName);
        context.Roles.Add(role);
        context.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = userId,
            RoleId = role.Id
        });
        context.SaveChanges();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
