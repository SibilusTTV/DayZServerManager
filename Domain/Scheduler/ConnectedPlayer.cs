using System.Text.Json.Serialization;

namespace Domain.Scheduler;

public class ConnectedPlayer
{
    public string Name { get; set; }
    public string Guid { get; set; }
    public int Id { get; set; }
    public int Ping { get; set; }
    public bool IsVerified { get; set; }
    public bool IsInLobby { get; set; }
    public string Ip { get; set; }

    public ConnectedPlayer()
    {
        
    }

    [JsonConstructor]
    public ConnectedPlayer(string name, string guid, int id, int ping, bool isVerified, bool isInLobby, string ip)
    {
        Name = name;
        Guid = guid;
        Id = id;
        Ping = ping;
        IsVerified = isVerified;
        IsInLobby = isInLobby;
        Ip = ip;
    }
}