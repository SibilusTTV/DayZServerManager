namespace Domain.Scheduler;

public class ServerPlayerInformation
{
    public string Id { get; set; }
    public string? ServerPlayerId { get; set; }
    public int? InstanceId { get; set; }
    public string Name { get; set; }
    public string Uid { get; set; }
    public bool IsVerified { get; set; }
    public string Ip { get; set; }
    public bool IsWhitelisted { get; set; }
    public bool IsBanned { get; set; }
    public string Role { get; set; }

    public ServerPlayerInformation()
    {
        
    }

    public ServerPlayerInformation(string id, string? serverPlayerId, string name, string uid, string ip, bool isVerified, bool isWhitelisted,
        bool isBanned, string role, int? instanceId)
    {
        Id = id;
        ServerPlayerId = serverPlayerId;
        Name = name;
        Uid = uid;
        Ip = ip;
        IsVerified = isVerified;
        IsWhitelisted = isWhitelisted;
        IsBanned = isBanned;
        Role = role;
        InstanceId = instanceId;
    }
}