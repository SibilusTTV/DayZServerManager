using System.ComponentModel.DataAnnotations.Schema;
using Domain.Manager;

namespace Domain.Scheduler;

public class SchedulerConfig
{
    public string Id { get; set; }
    
    [ForeignKey(nameof(Instance))]
    public string InstanceId { get; set; }
    
    public bool UseNickFilter { get; set; }
    public string FilteredNickMsg { get; set; }
    public List<string> BadNames { get; set; }
    public int Timeout { get; set; }

    public SchedulerConfig()
    {
        Id = Guid.NewGuid().ToString().ToLower();
        UseNickFilter = true;
        FilteredNickMsg = "You are using forbidden words in your user name";
        BadNames = new List<string>();
        Timeout = 60;
    }
}