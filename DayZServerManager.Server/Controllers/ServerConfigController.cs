
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServerConfigController : ControllerBase
    {
        private readonly ILogger<ManagerController> _logger;

        public ServerConfigController(ILogger<ManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetServerConfig")]
        public JsonResult GetServerConfig()
        {
            return new JsonResult(Manager.dayZServer.ServerConfig);
        }

        [HttpPost("PostServerConfig")]
        public void PostServerConfig([FromBody] ServerConfig config)
        {
            Manager.dayZServer.ServerConfig = config;
        }

        [HttpGet("SaveServerConfig")]
        public bool SaveServerConfig()
        {
            Manager.dayZServer.SaveServerConfig(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.serverConfigName));
            return true;
        }
    }
}
