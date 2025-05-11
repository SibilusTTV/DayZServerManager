using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses.PlayersDB;
using DayZServerManager.Server.Classes.SerializationClasses.StringInput;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DayZServerManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SchedulerController : ControllerBase
    {
        private readonly ILogger<ManagerController> _logger;
        public SchedulerController(ILogger<ManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetPlayers")]
        public JsonResult GetPlayers()
        {
            List<DatabasePlayerProps> players = new List<DatabasePlayerProps>();

            foreach (Player player in Manager.scheduler.PlayersDB.Players)
            {
                string bannedReasons = "";
                bool isBanned = false;
                Manager.scheduler.RconClient.BannedPlayers.ForEach(bannedPlayer => {
                    if (bannedPlayer.Guid == player.Guid) {
                        bannedReasons += bannedPlayer.Reason; isBanned = true;
                    }
                });

                players.Add(new DatabasePlayerProps(player.Name, player.Guid, player.Uid, player.IsVerified, player.IP, isBanned, bannedReasons, Manager.scheduler.WhitelistedPlayers.Contains(player.Uid)));
            }

            return new JsonResult(players);
        }

        [HttpPost("KickPlayer")]
        public void KickPlayer([FromBody] KickPlayerProps input)
        {
            Manager.scheduler?.KickPlayer(input.guid, input.reason, input.name);
        }

        [HttpPost("BanPlayer")]
        public void BanPlayer([FromBody] BanPlayerProps input)
        {
            Manager.scheduler?.BanPlayer(input.guid, input.reason, input.duration, input.name);
        }

        [HttpPost("UnbanPlayer")]
        public void UnbanPlayer([FromBody] UnbanPlayerProps input)
        {
            Manager.scheduler?.UnbanPlayer(input.guid, input.name);
        }

        [HttpPost("WhitelistPlayer")]
        public void WhitelistPlayer([FromBody] WhitelistPlayerProps input)
        {
            Manager.scheduler?.UnbanPlayer(input.guid, input.name);
            Manager.scheduler?.WhitelistPlayer(input.guid, input.name);
        }

        [HttpPost("UnwhitelistPlayer")]
        public void UnwhitelistPlayer([FromBody] UnwhitelistPlayerProps input)
        {
            Manager.scheduler?.UnwhitelistPlayer(input.guid, input.name);
        }

        [HttpPost("SendCommand")]
        public void SendCommand([FromBody] StringInput input)
        {
            Manager.scheduler?.SendCommand(input.value);
        }
    }
}
