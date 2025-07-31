using Microsoft.AspNetCore.Mvc;

namespace Sklad.Api.Controllers
{
    public class StorageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
