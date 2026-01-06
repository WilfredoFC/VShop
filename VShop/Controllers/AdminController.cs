using Microsoft.AspNetCore.Mvc;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;

namespace VShop.Controllers
{
    public class AdminController : Controller
    {
        
        public AdminController()
        {
            
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
