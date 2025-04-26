
using BytexDigital.BattlEye.Rcon.Domain;

namespace DayZServerManager.Server.Classes.Helpers
{
    public class ManagerProps
    {
        public string managerStatus {  get; set; }
        public string dayzServerStatus { get; set; }
        public string steamCMDStatus { get; set; }
        public int playersCount { get; set; }
        public List<PlayerProp> players {  get; set; }
        public ManagerProps(string _managerStatus, string _dayzServerStatus, string _steamCMDStatus, int _playersCount) 
        {
            managerStatus = _managerStatus;
            steamCMDStatus = _dayzServerStatus;
            dayzServerStatus = _dayzServerStatus;
            playersCount = _playersCount;
            players = new List<PlayerProp>();
        }
    }

    public class PlayerProp
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public int Id { get; set; }
        public int Ping { get; set; }
        public bool IsVerified { get; set; }
        public bool IsInLobby { get; set; }
        public string IP { get; set; }

        public PlayerProp(string _name, string _guid, int _id, int _ping, bool _isVerified, bool _isInLobby, string _ip)
        {
            Name = _name;
            Guid = _guid;
            Id = _id;
            Ping = _ping;
            IsVerified = _isVerified;
            IsInLobby = _isInLobby;
            IP = _ip;
        }

        public PlayerProp(Player player)
        {
            Name = player.Name;
            Guid = player.Guid;
            Id = player.Id;
            Ping = player.Ping;
            IsVerified = player.IsVerified;
            IsInLobby = player.IsInLobby;
            IP = player.RemoteEndpoint.ToString();
        }
    }
}
