using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityFile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.StringInput;
using System.Xml.Linq;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RarityEditorController : ControllerBase
    {
        private readonly ILogger<DayZServerController> _logger;

        public RarityEditorController(ILogger<DayZServerController> logger)
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
