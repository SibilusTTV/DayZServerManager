using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes;
using System.Numerics;
using DayZServerManager.Server.Classes.Handlers.SchedulerHandler;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.StringInput;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManagerController : ControllerBase
    {
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(ILogger<ManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetServerStatus")]
        public ManagerProps GetServerStatus()
        {
            int playerCount = 0;
            string adminLog = Manager.dayZServer.GetAdminLog();
            if (Manager.scheduler != null)
            {
                Manager.scheduler.GetPlayers();
                playerCount = Manager.scheduler.RconClient.PlayersCount;
            }
            
            Manager.props ??= new ManagerProps(Manager.STATUS_LISTENING, Manager.STATUS_NOT_RUNNING, Manager.STATUS_NOT_RUNNING, playerCount, string.Empty, adminLog);

            return Manager.props;
        }

        [HttpGet("StartServer")]
        public bool StartDayZServer()
        {
            if ((Manager.props.managerStatus == Manager.STATUS_LISTENING
                || Manager.props.managerStatus == Manager.STATUS_SERVER_STOPPED
                || Manager.props.managerStatus == Manager.STATUS_ERROR)
                && Manager.props.dayzServerStatus == Manager.STATUS_NOT_RUNNING)
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
            while (Manager.props.managerStatus == Manager.STATUS_STOPPING_SERVER ||
                (Manager.props.managerStatus != Manager.STATUS_LISTENING
                && Manager.props.managerStatus != Manager.STATUS_ERROR))
            {
                Thread.Sleep(1000);
            }
            if (Manager.props.managerStatus == Manager.STATUS_LISTENING)
            {
                return true;
            }
            else if (Manager.props.managerStatus == Manager.STATUS_ERROR)
            {
                Manager.KillServerProcesses();
                Manager.props.managerStatus = Manager.STATUS_LISTENING;
                return true;
            }
            else
            {
                return false;
            }
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

        [HttpGet("GetManagerLog")]
        public string GetManagerLog()
        {
            return Manager.GetManagerLog();
        }
    }
}
