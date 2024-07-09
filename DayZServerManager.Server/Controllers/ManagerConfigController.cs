using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Xml.Linq;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManagerConfigController : ControllerBase
    {
        private readonly ILogger<DayZServerController> _logger;

        public ManagerConfigController(ILogger<DayZServerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetManagerConfig")]
        public string GetManagerConfig()
        {
            return Manager.GetManagerConfig();
        }

        [HttpPost("PostManagerConfig")]
        public bool PostManagerConfig([FromBody] string config)
        {
            Manager.PostManagerConfig(config);
            Manager.SaveManagerConfig();
            return true;
        }
    }
}
