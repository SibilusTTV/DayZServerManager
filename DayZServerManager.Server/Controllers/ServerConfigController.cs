using Microsoft.AspNetCore.Mvc;

namespace DayZServerManager.Server.Controllers
{
    public class ServerConfigController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
