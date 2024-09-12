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
        public ManagerProps GetServerStatus()
        {
            if (Manager.props != null)
            {
                return Manager.props;
            }
            else
            {
                return new ManagerProps("Listening", "Not Running", "Not Running", 0);
            }
        }

        [HttpGet("StartServer")]
        public bool StartDayZServer()
        {
            if (Manager.props != null && (Manager.props.managerStatus == "Listening" || Manager.props.managerStatus == "Server Stopped") && Manager.props.dayzServerStatus == "Not Running")
            {
                Task startTask = new Task(() => { Manager.StartServer(); });
                startTask.Start();
                return true;
            }
            return false;
        }

        [HttpGet("StopServer")]
        public bool StopDayZServer()
        {
            Manager.StopServer();
            if (Manager.props != null)
            {

                while (Manager.props.managerStatus == "Stopping Server")
                {
                    Thread.Sleep(1000);
                }
                if (Manager.props.managerStatus == "Server Stopped")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        [HttpPost("SendSteamGuard")]
        public bool SendSteamGuard([FromBody] SteamGuard guard)
        {
            if (guard != null && guard.code != null)
            {
                string response = Manager.SetSteamGuard(guard.code);
                if (response != null && (response == "Error" || response == "DayZServer not running"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
