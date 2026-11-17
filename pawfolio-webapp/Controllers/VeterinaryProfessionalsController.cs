using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace pawfolio_webapp.Controllers
{
    [Authorize(Roles = "VeterinaryProfessional")]
    public class VeterinaryProfessionalsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
