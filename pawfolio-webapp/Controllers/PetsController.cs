using pawfolio_webapp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // to get the current logged in user
using pawfolio_webapp.Models;
using System.Linq; // for sorting queries (LINQ)
using Microsoft.AspNetCore.Authorization; // Restrict access to logged in users

namespace pawfolio_webapp.Controllers
{
    [Authorize] // Restrict access to logged in users
    public class PetsController : Controller
    {
        // db connection
        ApplicationDbContext _context;

        // constructor with dependency
        public PetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET
        public IActionResult Index()
        {
            // get the current logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // fetch list of pets that belong to this user 
            var pets = _context.Pets
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Name)
                .ToList();

            return View(pets);
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Create(Models.Pet pet)
        {
            if (!ModelState.IsValid)
            {
                return View(pet);
            }

            // connect the pet to the current logged in user
            pet.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // auto generated 

            // save to db
            _context.Pets.Add(pet);
            _context.SaveChanges();

            // refresh list
            return RedirectToAction("Index");
        }

        // GET
        public IActionResult Edit(int id)
        {
            // find pet by id
            var pet = _context.Pets.Find(id);

            // if invalid id
            if (pet == null)
            {
                return NotFound();
            }

            // show view but make sure the pet belongs to the current logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (pet.UserId != userId)
            {
                return Forbid(); // return 403 Forbidden if the pet does not belong to the user
            }

            return View(pet);
        }

        // POST
        [HttpPost]
        public IActionResult Edit(int id, Pet pet)
        {
            if (id != pet.Id)
            {
                return BadRequest();
            }

            // find the existing pet in the database
            var existingPet = _context.Pets.Find(id);

            if (existingPet == null)
            {
                return NotFound();
            }

            // check if the pet belongs to the current logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingPet.UserId != userId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(pet);
            }

            // now update the existing pet's properties with the new values
            existingPet.Name = pet.Name;
            existingPet.Breed = pet.Breed;
            existingPet.Age = pet.Age;
            existingPet.Gender = pet.Gender;


            // save to db
            _context.SaveChanges();

            // refresh list
            return RedirectToAction("Index");
        }

        // GET
        public IActionResult Delete(int id)
        {
            // find pet by id
            var pet = _context.Pets.Find(id);

            // if invalid id
            if (pet == null)
            {
                return NotFound();
            }

            // make sure this pet belongs to the logged in user 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (pet.UserId != userId)
            {
                return Forbid(); // return 403 Forbidden if the pet does not belong to the user
            }

            // delete
            _context.Pets.Remove(pet);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
