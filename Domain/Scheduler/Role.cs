using System.ComponentModel.DataAnnotations.Schema;
using Domain.Manager;

namespace Domain.Scheduler;

public class Role
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    [ForeignKey(nameof(Instance))]
    public int InstanceId { get; set; }
    
    // TODO: Add fields for role rights

    public Role()
    {
        
    }

    public Role(string name, int instanceId)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        InstanceId = instanceId;
    }
}