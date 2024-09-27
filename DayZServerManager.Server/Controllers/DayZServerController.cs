using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.Helpers;
using System.Numerics;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.StringInput;

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
            if (Manager.props == null)
            {
                Manager.props = new ManagerProps(Manager.STATUS_LISTENING, Manager.STATUS_NOT_RUNNING, Manager.STEAMCMD_STATUS_NOT_RUNNING, 0);
            }
            return Manager.props;
        }

        [HttpGet("StartServer")]
        public bool StartDayZServer()
        {
            if (Manager.props != null && (Manager.props.managerStatus == Manager.STATUS_LISTENING || Manager.props.managerStatus == Manager.STATUS_SERVER_STOPPED) && Manager.props.dayzServerStatus == Manager.STATUS_NOT_RUNNING)
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
                while (Manager.props.managerStatus == Manager.STATUS_STOPPING_SERVER)
                {
                    Thread.Sleep(1000);
                }
                if (Manager.props.managerStatus == Manager.STATUS_LISTENING)
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
        public bool SendSteamGuard([FromBody] StringInput guard)
        {
            if (guard != null && guard.value != null)
            {
                string response = Manager.SetSteamGuard(guard.value);
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
