using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Manager;

namespace Domain.Scheduler;

public class ServerPlayer
{
    public string Id { get; set; }
    
    [ForeignKey(nameof(Instance))]
    public string InstanceId { get; set; }
    public Instance Instance { get; set; }
    
    [ForeignKey(nameof(Player))]
    public string PlayerId { get; set; }
    public Player Player { get; set; }
    
    [ForeignKey(nameof(Ban))]
    public string? BanId { get; set; }
    public Ban? Ban { get; set; }
    
    public bool IsWhitelisted { get; set; }
    public bool IsBanned { get; set; }
    
    [ForeignKey(nameof(Role))]
    public string RoleId { get; set; }
    public Role Role { get; set; }

    public ServerPlayer()
    {
        
    }
    
    public ServerPlayer(string instanceId, string playerId, bool isWhitelisted, bool isBanned, string roleId)
    {
        this.Id = Guid.NewGuid().ToString().ToLower();
        InstanceId = instanceId;
        PlayerId = playerId;
        IsWhitelisted = isWhitelisted;
        IsBanned = isBanned;
        RoleId = roleId;
    }
}