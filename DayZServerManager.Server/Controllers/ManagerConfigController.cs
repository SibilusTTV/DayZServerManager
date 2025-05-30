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
        private readonly ILogger<ManagerController> _logger;

        public ManagerConfigController(ILogger<ManagerController> logger)
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
            Manager.PostManagerConfig(config);
            Manager.props.managerStatus = Manager.STATUS_LISTENING;
            return true;
        }
    }
}
