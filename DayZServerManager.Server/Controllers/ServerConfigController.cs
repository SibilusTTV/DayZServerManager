using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServerConfigController : ControllerBase
    {
        private readonly ILogger<DayZServerController> _logger;

        public ServerConfigController(ILogger<DayZServerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetServerConfig")]
        public string GetServerConfig()
        {
            return Manager.GetServerConfig();
        }

        [HttpPost("PostServerConfig")]
        public void PostServerConfig([FromBody] string config)
        {
            Manager.PostServerConfig(config);
        }
    }
}
