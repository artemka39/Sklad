using Microsoft.AspNetCore.Mvc;

namespace Sklad.Api.Controllers
{
    public class ShipmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
