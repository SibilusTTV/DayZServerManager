using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Manager;

public class InstanceServerMod
{
    [ForeignKey(nameof(Instance))]
    public int InstanceId { get; set; }
    
    [ForeignKey(nameof(Mod))]
    public string ModId { get; set; }
    
    public Mod Mod { get; set; }
    
    public int Position { get; set; }

    public InstanceServerMod()
    {
        
    }

    public InstanceServerMod(int instanceId, Mod mod, int position)
    {
        InstanceId = instanceId;
        ModId = mod.id;
        Mod = mod;
        Position = position;
    }
}