using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Scheduler;

public class Player
{
    [Key]
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Uid { get; set; }
    public bool IsVerified { get; set; }
    public string Ip { get; set; }

    public Player()
    {
        
    }
    
    [JsonConstructor]
    public Player(Guid guid, string name, string uid, bool isVerified, string ip)
    {
        Guid = guid;
        Name = name;
        Uid = uid;
        IsVerified = isVerified;
        Ip = ip;
    }
}