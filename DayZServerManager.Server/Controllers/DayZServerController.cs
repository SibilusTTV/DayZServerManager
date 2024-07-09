using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DayZServerController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<DayZServerController> _logger;

        public DayZServerController(ILogger<DayZServerController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetDayZServerStatus")]
        public string Get()
        {
            if (Manager.server.CheckServer())
            {
                return "Server still running";
            }
            else
            {
                return "Server is not running";
            }

        }

        [HttpGet(Name = "StartDayZServer")]
        [Route("Start")]
        public string StartDayZServer()
        {
            if (Manager.server != null)
            {

            }
            return "Server was not started, because fuck you";
        }
    }
}
