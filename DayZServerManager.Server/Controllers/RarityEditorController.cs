using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityFile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using DayZServerManager.Server.Classes.Handlers.ServerHandler.RarityHandler;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RarityEditorController : ControllerBase
    {
        private readonly ILogger<ManagerController> _logger;

        public RarityEditorController(ILogger<ManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetRarityFile/{name}")]
        public RarityFile? GetRarityFile(string name)
        {
            if (name != null)
            {
                return RarityEditor.GetRarityFile(name);
            }
            return null;
        }

        [HttpPost("PostRarityFile/{name}")]
        public bool PostRarityFile([FromBody] RarityFile rarityFile, string name)
        {
            if (rarityFile != null && name != null)
            {
                RarityEditor.UpdateRaritiesAndTypes(name, rarityFile);
                return true;
            }
            return false;
        }
    }
}
