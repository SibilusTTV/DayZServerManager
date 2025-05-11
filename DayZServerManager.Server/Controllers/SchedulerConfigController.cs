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
        private readonly ILogger<ManagerController> _logger;

        public SchedulerConfigController(ILogger<ManagerController> logger)
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
    }
}
