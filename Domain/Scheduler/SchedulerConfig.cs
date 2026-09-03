using System.ComponentModel.DataAnnotations.Schema;
using Domain.Manager;

namespace Domain.Scheduler;

public class SchedulerConfig
{
    public string Id { get; set; }
    
    [ForeignKey(nameof(Instance))]
    public int InstanceId { get; set; }
    
    public bool UseNickFilter { get; set; }
    public string FilteredNickMsg { get; set; }
    public List<string> BadNames { get; set; }
    public int Timeout { get; set; }
    public bool restartOnUpdate { get; set; }
    public int restartInterval { get; set; }
    public List<CustomMessage> customMessages { get; set; }

    public SchedulerConfig()
    {
        
    }

    public SchedulerConfig(int instanceId)
    {
        Id = Guid.NewGuid().ToString().ToLower();
        InstanceId = instanceId;
        UseNickFilter = false;
        FilteredNickMsg = "";
        BadNames = [];
        Timeout = 60;
        restartOnUpdate = true;
        restartInterval = 4;
        customMessages = [];
        customMessages.Add(new CustomMessage(false, 0, new TimeSpan(0, 5, 0), new TimeSpan(0, 15, 0), "Need Help?",
            "Make sure to join our Discord", "ExclamationMark", ""));
    }
}