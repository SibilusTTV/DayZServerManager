using Microsoft.AspNetCore.Components.Web;
using System.Text.Json.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses.PlayersDB
{
    public class Player
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public string Uid { get; set; }
        public bool IsVerified { get; set; }
        public string IP { get; set; }

        [JsonConstructor]
        public Player(string name, string guid, string uid, bool isVerified, string ip)
        {
            Name = name;
            Guid = guid;
            Uid = uid;
            IsVerified = isVerified;
            IP = ip;
        }
    }
}
