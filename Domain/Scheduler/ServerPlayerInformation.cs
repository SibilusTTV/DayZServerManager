namespace Domain.Scheduler;

public class ServerPlayerInformation
{
    public Guid Id { get; set; }
    public Guid ServerPlayerId { get; set; }
    public string Name { get; set; }
    public string Uid { get; set; }
    public bool IsVerified { get; set; }
    public string Ip { get; set; }
    public bool IsWhitelisted { get; set; }
    public bool IsBanned { get; set; }
    public string Role { get; set; }
    public Guid InstanceId { get; set; }

    public ServerPlayerInformation(Guid id, Guid serverPlayerId, string name, string uid, string ip, bool isVerified, bool isWhitelisted,
        bool isBanned, string role, Guid instanceId)
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