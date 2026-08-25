using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Manager;

namespace Domain.Scheduler;

public class ServerPlayer
{
    public Guid Id { get; set; }
    
    [ForeignKey(nameof(Instance))]
    public Guid InstanceId { get; set; }
    public Instance Instance { get; set; }
    
    [ForeignKey(nameof(Player))]
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
    
    [ForeignKey(nameof(Ban))]
    public Guid? BanId { get; set; }
    public Ban? Ban { get; set; }
    
    public bool IsWhitelisted { get; set; }
    public bool IsBanned { get; set; }
    public string Role { get; set; }

    public ServerPlayer()
    {
        
    }
    
    public ServerPlayer(Guid id, Guid instanceId, Guid playerId, bool isWhitelisted, bool isBanned, string role)
    {
        Id = id;
        InstanceId = instanceId;
        PlayerId = playerId;
        IsWhitelisted = isWhitelisted;
        IsBanned = isBanned;
        Role = role;
    }
}