using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DayZServerController : ControllerBase
    {
        private readonly ILogger<DayZServerController> _logger;

        public DayZServerController(ILogger<DayZServerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetServerStatus")]
        public bool GetServerStatus()
        {
            if (Manager.server != null && Manager.server.CheckServer())
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        [HttpGet("GetBECStatus")]
        public bool GetBECStatus()
        {
            if (Manager.server != null && Manager.server.CheckBEC())
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        [HttpGet("StartServer")]
        public string StartDayZServer()
        {
            Manager.StartServer();
            return "Server was not started, because fuck you";
        }

        [HttpGet("StopServer")]
        public bool StopDayZServer()
        {
            Manager.KillServerProcesses();
            return true;
        }
    }
}
