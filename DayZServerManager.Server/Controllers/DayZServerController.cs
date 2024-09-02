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
        public string GetServerStatus()
        {
            if (Manager.props != null)
            {
                return Manager.props._serverStatus;
            }
            else
            {
                return "Server not started";
            }
        }

        [HttpGet("GetSchedulerStatus")]
        public bool GetSchedulerStatus()
        {
            if (Manager.dayZServer != null && Manager.dayZServer.CheckScheduler())
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        [HttpGet("StartServer")]
        public bool StartDayZServer()
        {
            if (Manager.props == null || (Manager.props._serverStatus == "Server stopped" || Manager.props._serverStatus == "Waiting for Starting" || Manager.props._serverStatus == "Error" || Manager.props._serverStatus == "Waiting for Starting" || Manager.props._serverStatus == "Server not running" || Manager.props._serverStatus == "Please set Username and Password"))
            {
                Manager.props = new ManagerProps("Starting");
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
                while (Manager.props._serverStatus == "Stopping Server")
                {
                    Thread.Sleep(1000);
                }
                if (Manager.props._serverStatus == "Server stopped")
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
        public string SendSteamGuard([FromBody] SteamGuard guard)
        {
            if (guard != null && guard.code != null)
            {
                return Manager.SetSteamGuard(guard.code);
            }
            else
            {
                return "Invalid input";
            }
        }
    }
}
