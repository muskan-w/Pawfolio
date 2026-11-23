using pawfolio_webapp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // to get the current logged in user
using pawfolio_webapp.Models;
using System.Linq; // for sorting queries (LINQ)
using System.Text;
using Microsoft.AspNetCore.Authorization; // Restrict access to logged in users

namespace pawfolio_webapp.Controllers
{
    [Authorize] // Restrict access to logged in users
    public class PetsController : Controller
    {
        private const int MaxPictureSize = 5 * 1024 * 1024;
        private readonly ApplicationDbContext _context;

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

        [HttpGet]
        public IActionResult Profile(int id)
        {
            var pet = FindOwnedPet(id);
            return pet == null ? NotFound() : View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(int id, IFormFile? picture)
        {
            var pet = FindOwnedPet(id);
            if (pet == null)
            {
                return NotFound();
            }

            if (picture == null || picture.Length == 0)
            {
                TempData["ProfileError"] = "Choose an image to upload.";
                return RedirectToAction(nameof(Profile), new { id });
            }

            if (picture.Length > MaxPictureSize)
            {
                TempData["ProfileError"] = "The image must be 5 MB or smaller.";
                return RedirectToAction(nameof(Profile), new { id });
            }

            using var memoryStream = new MemoryStream();
            await picture.CopyToAsync(memoryStream);
            var imageData = memoryStream.ToArray();
            var contentType = GetImageContentType(imageData);

            if (contentType == null)
            {
                TempData["ProfileError"] = "Upload a valid PNG, JPEG, GIF, or WebP image.";
                return RedirectToAction(nameof(Profile), new { id });
            }

            pet.ImageData = imageData;
            pet.ImageContentType = contentType;
            await _context.SaveChangesAsync();

            TempData["ProfileMessage"] = "Profile picture updated.";
            return RedirectToAction(nameof(Profile), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCareInfo(int id, string section, string? value)
        {
            var pet = FindOwnedPet(id);
            if (pet == null)
            {
                return NotFound();
            }

            if (value?.Length > 1000)
            {
                TempData["ProfileError"] = "Care information must be 1,000 characters or fewer.";
                return RedirectToAction(nameof(Profile), new { id });
            }

            var careInfo = value?.Trim();
            if (string.IsNullOrEmpty(careInfo))
            {
                careInfo = null;
            }

            switch (section)
            {
                case "weight":
                    pet.Weight = careInfo;
                    break;
                case "food":
                    pet.Food = careInfo;
                    break;
                case "medication":
                    pet.Medication = careInfo;
                    break;
                case "vaccination":
                    pet.Vaccination = careInfo;
                    break;
                default:
                    return BadRequest();
            }

            await _context.SaveChangesAsync();
            TempData["ProfileMessage"] = "Care information saved.";
            return RedirectToAction(nameof(Profile), new { id });
        }

        [HttpGet]
        public IActionResult DownloadProfile(int id)
        {
            var pet = FindOwnedPet(id);
            if (pet == null)
            {
                return NotFound();
            }

            var contents = string.Join(Environment.NewLine,
                $"Name: {pet.Name}",
                $"Species: {pet.Species}",
                $"Breed: {pet.Breed}",
                $"Age: {pet.Age}",
                $"Gender: {pet.Gender}",
                $"Weight: {pet.Weight ?? "Not provided"}",
                $"Food: {pet.Food ?? "Not provided"}",
                $"Medication: {pet.Medication ?? "Not provided"}",
                $"Vaccination: {pet.Vaccination ?? "Not provided"}");
            var fileName = string.Concat(pet.Name.Select(character =>
                Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"pet-{pet.Id}";
            }

            return File(Encoding.UTF8.GetBytes(contents), "text/plain", $"{fileName}-profile.txt");
        }

        private Pet? FindOwnedPet(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userId)
                ? null
                : _context.Pets.FirstOrDefault(pet => pet.Id == id && pet.UserId == userId);
        }

        private static string? GetImageContentType(byte[] data)
        {
            if (data.AsSpan().StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            {
                return "image/png";
            }

            if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return "image/jpeg";
            }

            if (data.AsSpan().StartsWith("GIF87a"u8) || data.AsSpan().StartsWith("GIF89a"u8))
            {
                return "image/gif";
            }

            if (data.Length >= 12 && data.AsSpan(0, 4).SequenceEqual("RIFF"u8) && data.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            {
                return "image/webp";
            }

            return null;
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
            existingPet.Species = pet.Species;


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
