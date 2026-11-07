using Microsoft.AspNetCore.Mvc;

namespace pawfolio_webapp.Controllers
{
    public class VeterinaryProfessionalsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
