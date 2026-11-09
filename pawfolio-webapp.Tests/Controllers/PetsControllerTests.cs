using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pawfolio_webapp.Controllers;
using pawfolio_webapp.Data;
using pawfolio_webapp.Models;

namespace pawfolio_webapp.Tests.Controllers;

[TestClass]
public class PetsControllerTests
{
    [TestMethod]
    public void Index_ReturnsOnlyCurrentUsersPetsOrderedByName()
    {
        using var context = CreateContext();

        context.Pets.AddRange(
            new Pet { Name = "Ziggy", Breed = "Poodle", Age = 4, Gender = "Male", UserId = "user-1" },
            new Pet { Name = "Bella", Breed = "Beagle", Age = 3, Gender = "Female", UserId = "user-1" },
            new Pet { Name = "Other", Breed = "Labrador", Age = 5, Gender = "Male", UserId = "user-2" });

        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Index();

        var view = Assert.IsInstanceOfType<ViewResult>(result);
        var pets = Assert.IsInstanceOfType<List<Pet>>(view.Model);

        CollectionAssert.AreEqual(
            new[] { "Bella", "Ziggy" },
            pets.Select(pet => pet.Name).ToArray());
        Assert.IsTrue(pets.All(pet => pet.UserId == "user-1"));
    }

    [TestMethod]
    public void Create_Get_ReturnsView()
    {
        using var context = CreateContext();
        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Create();

        Assert.IsInstanceOfType<ViewResult>(result);
    }

    [TestMethod]
    public void Create_Post_WithValidModel_SavesPetForCurrentUserAndRedirects()
    {
        using var context = CreateContext();
        var controller = CreateController(context, CreateUser("user-1"));
        var pet = new Pet
        {
            Name = "Max",
            Breed = "Labrador",
            Age = 5,
            Gender = "Male"
        };

        var result = controller.Create(pet);

        var redirect = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.AreEqual("Index", redirect.ActionName);

        var savedPet = context.Pets.Single();
        Assert.AreEqual("Max", savedPet.Name);
        Assert.AreEqual("Labrador", savedPet.Breed);
        Assert.AreEqual(5, savedPet.Age);
        Assert.AreEqual("Male", savedPet.Gender);
        Assert.AreEqual("user-1", savedPet.UserId);
    }

    [TestMethod]
    public void Create_Post_WithInvalidModelState_ReturnsViewWithPet()
    {
        using var context = CreateContext();
        var controller = CreateController(context, CreateUser("user-1"));
        controller.ModelState.AddModelError("Name", "Name is required");

        var pet = new Pet
        {
            Name = string.Empty,
            Breed = "Labrador",
            Age = 5,
            Gender = "Male"
        };

        var result = controller.Create(pet);

        var view = Assert.IsInstanceOfType<ViewResult>(result);
        Assert.AreSame(pet, view.Model);
        Assert.AreEqual(0, context.Pets.Count());
    }

    [TestMethod]
    public void Edit_Get_WhenPetDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Edit(123);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public void Edit_Get_WhenPetBelongsToAnotherUser_ReturnsForbid()
    {
        using var context = CreateContext();
        context.Pets.Add(new Pet
        {
            Id = 1,
            Name = "Bella",
            Breed = "Beagle",
            Age = 3,
            Gender = "Female",
            UserId = "user-2"
        });
        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Edit(1);

        Assert.IsInstanceOfType<ForbidResult>(result);
    }

    [TestMethod]
    public void Edit_Get_WhenPetBelongsToCurrentUser_ReturnsViewWithPet()
    {
        using var context = CreateContext();
        var pet = new Pet
        {
            Id = 1,
            Name = "Bella",
            Breed = "Beagle",
            Age = 3,
            Gender = "Female",
            UserId = "user-1"
        };
        context.Pets.Add(pet);
        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Edit(1);

        var view = Assert.IsInstanceOfType<ViewResult>(result);
        var returnedPet = Assert.IsInstanceOfType<Pet>(view.Model);
        Assert.AreEqual("Bella", returnedPet.Name);
    }

    [TestMethod]
    public void Edit_Post_WhenIdsDoNotMatch_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Edit(
            1,
            new Pet
            {
                Id = 2,
                Name = "Bella",
                Breed = "Beagle",
                Age = 3,
                Gender = "Female"
            });

        Assert.IsInstanceOfType<BadRequestResult>(result);
    }

    [TestMethod]
    public void Edit_Post_WhenPetDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Edit(
            1,
            new Pet
            {
                Id = 1,
                Name = "Bella",
                Breed = "Beagle",
                Age = 3,
                Gender = "Female"
            });

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public void Edit_Post_WhenPetBelongsToAnotherUser_ReturnsForbid()
    {
        using var context = CreateContext();
        context.Pets.Add(new Pet
        {
            Id = 1,
            Name = "Bella",
            Breed = "Beagle",
            Age = 3,
            Gender = "Female",
            UserId = "user-2"
        });
        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Edit(
            1,
            new Pet
            {
                Id = 1,
                Name = "Updated",
                Breed = "Poodle",
                Age = 4,
                Gender = "Female"
            });

        Assert.IsInstanceOfType<ForbidResult>(result);
    }

    [TestMethod]
    public void Edit_Post_WithInvalidModelState_ReturnsViewWithoutSavingChanges()
    {
        using var context = CreateContext();
        context.Pets.Add(new Pet
        {
            Id = 1,
            Name = "Bella",
            Breed = "Beagle",
            Age = 3,
            Gender = "Female",
            UserId = "user-1"
        });
        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));
        controller.ModelState.AddModelError("Name", "Name is required");

        var editedPet = new Pet
        {
            Id = 1,
            Name = string.Empty,
            Breed = "Poodle",
            Age = 4,
            Gender = "Female"
        };

        var result = controller.Edit(1, editedPet);

        var view = Assert.IsInstanceOfType<ViewResult>(result);
        Assert.AreSame(editedPet, view.Model);

        var storedPet = context.Pets.Single();
        Assert.AreEqual("Bella", storedPet.Name);
        Assert.AreEqual("Beagle", storedPet.Breed);
        Assert.AreEqual(3, storedPet.Age);
    }

    [TestMethod]
    public void Edit_Post_WithValidModel_UpdatesPetAndRedirects()
    {
        using var context = CreateContext();
        context.Pets.Add(new Pet
        {
            Id = 1,
            Name = "Bella",
            Breed = "Beagle",
            Age = 3,
            Gender = "Female",
            UserId = "user-1"
        });
        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Edit(
            1,
            new Pet
            {
                Id = 1,
                Name = "Bella Updated",
                Breed = "Poodle",
                Age = 4,
                Gender = "Female"
            });

        var redirect = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.AreEqual("Index", redirect.ActionName);

        var updatedPet = context.Pets.Single();
        Assert.AreEqual("Bella Updated", updatedPet.Name);
        Assert.AreEqual("Poodle", updatedPet.Breed);
        Assert.AreEqual(4, updatedPet.Age);
        Assert.AreEqual("Female", updatedPet.Gender);
        Assert.AreEqual("user-1", updatedPet.UserId);
    }

    [TestMethod]
    public void Delete_WhenPetDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Delete(123);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public void Delete_WhenPetBelongsToAnotherUser_ReturnsForbid()
    {
        using var context = CreateContext();
        context.Pets.Add(new Pet
        {
            Id = 1,
            Name = "Bella",
            Breed = "Beagle",
            Age = 3,
            Gender = "Female",
            UserId = "user-2"
        });
        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Delete(1);

        Assert.IsInstanceOfType<ForbidResult>(result);
        Assert.AreEqual(1, context.Pets.Count());
    }

    [TestMethod]
    public void Delete_WhenPetBelongsToCurrentUser_RemovesPetAndRedirects()
    {
        using var context = CreateContext();
        context.Pets.Add(new Pet
        {
            Id = 1,
            Name = "Bella",
            Breed = "Beagle",
            Age = 3,
            Gender = "Female",
            UserId = "user-1"
        });
        context.SaveChanges();

        var controller = CreateController(context, CreateUser("user-1"));

        var result = controller.Delete(1);

        var redirect = Assert.IsInstanceOfType<RedirectToActionResult>(result);
        Assert.AreEqual("Index", redirect.ActionName);
        Assert.AreEqual(0, context.Pets.Count());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PetsController CreateController(
        ApplicationDbContext context,
        ClaimsPrincipal user)
    {
        return new PetsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            }
        };
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