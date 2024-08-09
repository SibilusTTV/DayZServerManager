using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.Helpers;

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
            if (Manager.dayZServer != null && Manager.dayZServer.CheckServer())
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
            if (Manager.dayZServer != null && Manager.dayZServer.CheckBEC())
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
            Manager.props = new ManagerProps("Starting", "");
            Task startTask = new Task(() => { Manager.StartServer(); });
            startTask.Start();
            if (startTask != null)
            {
                while (Manager.dayZServer == null)
                {
                    Thread.Sleep(1000);
                }
                while (Manager.props._updateStatus != "Server downloaded" && Manager.props._updateStatus != "Error")
                {
                    if (Manager.props._updateStatus == "Steam Guard")
                    {
                        return "Steam Guard";
                    }
                    Thread.Sleep(1000);
                }

                while (Manager.props._updateStatus != "Mods downloaded" && Manager.props._updateStatus != "Error")
                {
                    if (Manager.props._updateStatus == "Steam Guard")
                    {
                        return "Steam Guard";
                    }
                    Thread.Sleep(1000);
                }

                return "";
            }
            else
            {
                return Manager.props._serverStatus;
            }
        }

        [HttpGet("StopServer")]
        public bool StopDayZServer()
        {
            Manager.KillServerProcesses();
            return true;
        }

        [HttpPost("SendSteamGuard")]
        public string SendSteamGuard([FromBody] SteamGuard guard)
        {
            return Manager.SetSteamGuard(guard.code);
        }
    }
}
