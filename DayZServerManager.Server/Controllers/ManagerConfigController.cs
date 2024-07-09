using Microsoft.AspNetCore.Mvc;

namespace DayZServerManager.Server.Controllers
{
    public class ManagerConfigController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
