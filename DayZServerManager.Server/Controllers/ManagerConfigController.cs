﻿using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
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
        public JsonResult GetManagerConfig()
        {
            return new JsonResult(Manager.managerConfig);
        }

        [HttpPost("PostManagerConfig")]
        public bool PostManagerConfig([FromBody] ManagerConfig config)
        {
            Manager.managerConfig = config;
            Manager.SaveManagerConfig();
            if (Manager.props != null)
            {
                Manager.props.managerStatus = Manager.STATUS_LISTENING;
            }
            return true;
        }
    }
}
