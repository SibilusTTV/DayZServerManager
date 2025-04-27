using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.Helpers;
using System.Numerics;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.StringInput;
using DayZServerManager.Server.Classes.Handlers.SchedulerHandler;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses;

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
            int playerCount = 0;
            string adminLog = Manager.GetAdminLog();
            if (Manager.dayZServer != null && Manager.dayZServer.scheduler != null)
            {
                playerCount = Manager.dayZServer.scheduler.GetPlayers();
            }
            
            Manager.props ??= new ManagerProps(Manager.STATUS_LISTENING, Manager.STATUS_NOT_RUNNING, Manager.STATUS_NOT_RUNNING, playerCount, adminLog);

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

        [HttpPost("KickPlayer")]
        public void KickPlayer([FromBody] RemovePlayerInput input)
        {
            Manager.dayZServer?.scheduler?.KickPlayer(input.id, input.reason, input.name);
        }

        [HttpPost("BanPlayer")]
        public void BanPlayer([FromBody] RemovePlayerInput input)
        {
            Manager.dayZServer?.scheduler?.BanPlayer(input.id, input.reason, input.duration, input.name);
        }

        [HttpPost("SendCommand")]
        public void SendCommand([FromBody] StringInput input)
        {
            Manager.dayZServer?.scheduler?.SendCommand(input.value);
        }

        [HttpGet("GetManagerLog")]
        public string GetManagerLog()
        {
            return Manager.managerLog;
        }
    }
}
