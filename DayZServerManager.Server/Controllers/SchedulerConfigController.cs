using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SchedulerConfigController : ControllerBase
    {
        private readonly ILogger<DayZServerController> _logger;

        public SchedulerConfigController(ILogger<DayZServerController> logger)
        {
            _logger = logger;
        }


        [HttpGet("GetSchedulerConfig")]
        public JsonResult GetSchedulerConfig()
        {
            return new JsonResult(Manager.scheduler.Config);
        }

        [HttpPost("PostSchedulerConfig")]
        public void PostServerConfig([FromBody] SchedulerConfig config)
        {
            Manager.scheduler.SaveSchedulerConfig(config);
        }

        [HttpGet("GetPlayers")]
        public JsonResult GetPlayers()
        {
            return new JsonResult(Manager.scheduler.PlayersDB);
        }

        [HttpGet("GetBannedPlayers")]
        public JsonResult GetBannedPlayers()
        {
            return new JsonResult(Manager.scheduler.RconClient.BannedPlayers);
        }

    }
}
