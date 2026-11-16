using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using pawfolio_webapp.Data;
using pawfolio_webapp.Models;

namespace pawfolio_webapp.Controllers
{
    public class FileController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FileController(ApplicationDbContext context) => _context = context;

        [Authorize]
        // GET: File/Upload
        public IActionResult Upload() => View();

        // POST: File/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file selected");
            }
            //read file content into byte array
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            //create model/save to db
            var fileModel = new FileModel
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Data = fileData
            };

            _context.Files.Add(fileModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //GET: View by ID
        [HttpGet("View/{id}")]
        public async Task<IActionResult> ViewFile(int id)
        {
            var file = await _context.Files.FindAsync(id);

            if(file == null)
            {
                return NotFound("File not found");
            }

            return File(file.Data, file.ContentType);
        }

        //GET: Download by ID
        [HttpGet("Download{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if(file == null)
            {
                return NotFound("File not found");
            }

            return File(file.Data, file.ContentType, file.FileName);
        }

    }
}
