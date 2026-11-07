using Microsoft.AspNetCore.Mvc;

namespace pawfolio_webapp.Controllers
{
    public class PetOwnersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
