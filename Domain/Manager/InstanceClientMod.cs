using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Manager;

public class InstanceClientMod
{
    [ForeignKey(nameof(Instance))]
    public Guid InstanceId { get; set; }
    
    public Instance Instance { get; set; }
    
    [ForeignKey(nameof(Mod))]
    public Guid ModId { get; set; }
    
    public Mod Mod { get; set; }
}