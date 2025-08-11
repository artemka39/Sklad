using Microsoft.AspNetCore.Mvc;

namespace Sklad.Api.Controllers
{
    public class ReceiptController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
