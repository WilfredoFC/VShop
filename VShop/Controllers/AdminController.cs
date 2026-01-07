using Microsoft.AspNetCore.Mvc;

namespace VShop.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Productos");
        }
    }
}
