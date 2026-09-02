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

    public SchedulerConfig()
    {
        
    }

    public SchedulerConfig(int instanceId, bool useNickFilter, string filteredNickMsg, List<string> badNames, int timeout)
    {
        Id = Guid.NewGuid().ToString().ToLower();
        InstanceId = instanceId;
        UseNickFilter = useNickFilter;
        FilteredNickMsg = filteredNickMsg;
        BadNames = badNames;
        Timeout = timeout;
    }
}