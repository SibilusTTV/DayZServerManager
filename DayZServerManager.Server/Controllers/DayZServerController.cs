using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.Helpers;
using System.Numerics;

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
            Manager.props = new ManagerProps("Starting");
            Task startTask = new Task(() => { Manager.StartServer(); });
            startTask.Start();
            if (startTask != null)
            {
                while (startTask.IsCompleted || (Manager.props._serverStatus != "Error" && Manager.props._serverStatus != "Please set Username and Password" && Manager.props._serverStatus != "Server Started" && Manager.props._serverStatus != "Steam Guard" && Manager.props._serverStatus != "Server Stopped"))
                {
                    Thread.Sleep(5000);
                }

                switch (Manager.props._serverStatus)
                {
                    case "Please set Username and Password":
                        return "Please set Username and Password";
                    case "Steam Guard":
                        return "Steam Guard";
                    case "Server Started":
                        return "Server Started";
                    case "Error":
                        return "Error";
                    case "Server Stopped":
                        return "Server Stopped";
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
            Manager.StopServer();
            return true;
        }

        [HttpPost("SendSteamGuard")]
        public string SendSteamGuard([FromBody] SteamGuard guard)
        {
            return Manager.SetSteamGuard(guard.code);
        }
    }
}
