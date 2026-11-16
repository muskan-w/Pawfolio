using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pawfolio_webapp.Controllers;
using pawfolio_webapp.Data;
using pawfolio_webapp.Models;

namespace pawfolio_webapp.Tests;

[TestClass]
public class PetOwnersControllerTests
{
    [TestMethod]
    public void IndexReturnsEmptyViewForAnonymousUser()
    {
        using var context = CreateContext();
        var controller = CreateController(
            context,
            new ClaimsPrincipal(new ClaimsIdentity()));

        var result = controller.Index();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pets = Assert.IsInstanceOfType<List<Pet>>(viewResult.Model);

        Assert.AreEqual(0, pets.Count);
    }

    [TestMethod]
    public void IndexReturnsOnlyPetsBelongingToCurrentUser()
    {
        using var context = CreateContext();

        context.Pets.AddRange(
            new Pet { Name = "Max", Breed = "Labrador", Age = 5, Gender = "Male", UserId = "user-1" },
            new Pet { Name = "Bella", Breed = "Beagle", Age = 3, Gender = "Female", UserId = "user-2" });

        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Index();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pets = Assert.IsInstanceOfType<List<Pet>>(viewResult.Model);

        Assert.AreEqual(1, pets.Count);
        Assert.AreEqual("Max", pets[0].Name);
    }

    [TestMethod]
    public void IndexOrdersCurrentUsersPetsByName()
    {
        using var context = CreateContext();

        context.Pets.AddRange(
            new Pet { Name = "Ziggy", Breed = "Poodle", Age = 4, Gender = "Male", UserId = "user-1" },
            new Pet { Name = "Bella", Breed = "Beagle", Age = 3, Gender = "Female", UserId = "user-1" },
            new Pet { Name = "Milo", Breed = "Tabby", Age = 2, Gender = "Male", UserId = "user-1" });

        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Index();

        var viewResult = Assert.IsInstanceOfType<ViewResult>(result);
        var pets = Assert.IsInstanceOfType<List<Pet>>(viewResult.Model);

        CollectionAssert.AreEqual(
            new[] { "Bella", "Milo", "Ziggy" },
            pets.Select(pet => pet.Name).ToArray());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PetOwnersController CreateController(
        ApplicationDbContext context,
        ClaimsPrincipal user)
    {
        var controller = new PetOwnersController(context)
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

    private static ClaimsPrincipal CreateUser(string userId)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            },
            authenticationType: "TestAuthentication");

        return new ClaimsPrincipal(identity);
    }
}
