using System.Text.Json.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses
{
    public class ConnectedPlayer
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public int Id { get; set; }
        public int Ping { get; set; }
        public bool IsVerified { get; set; }
        public bool IsInLobby { get; set; }
        public string IP { get; set; }

        [JsonConstructor]
        public ConnectedPlayer(string name, string guid, int id, int ping, bool isVerified, bool isInLobby, string ip)
        {
            Name = name;
            Guid = guid;
            Id = id;
            Ping = ping;
            IsVerified = isVerified;
            IsInLobby = isInLobby;
            IP = ip;
        }
    }
}
