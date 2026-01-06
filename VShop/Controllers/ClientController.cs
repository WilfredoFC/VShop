using Microsoft.AspNetCore.Mvc;

namespace VShop.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
