using System.ComponentModel.DataAnnotations.Schema;
using Domain.Manager;

namespace Domain.Scheduler;

public class Ban
{
    public string Id { get; set; }
    public int BanId { get; set; }
    public int RemainingTime { get; set; }
    public string Reason { get; set; }
    
    public Ban()
    {
        
    }

    public Ban(int banId, int remainingTime, string reason)
    {
        Id = Guid.NewGuid().ToString().ToLower();
        BanId = banId;
        RemainingTime = remainingTime;
        Reason = reason;
    }
}