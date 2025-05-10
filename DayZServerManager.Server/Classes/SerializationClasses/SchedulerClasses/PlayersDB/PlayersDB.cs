
using DayZServerManager.Server.Classes.Handlers.SchedulerHandler;
using DayZServerManager.Server.Classes.Helpers;
using System.Text.Json.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses.PlayersDB
{
    public class PlayersDB
    {
        public List<Player> Players { get; set; }

        public PlayersDB()
        {
            Players = new List<Player>();
        }

        [JsonConstructor]
        public PlayersDB(List<Player> players)
        {
            Players = players;
        }
    }
}
