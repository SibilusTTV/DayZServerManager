using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Scheduler;

public class Player
{
    [Key]
    public string Guid { get; set; }
    public string Name { get; set; }
    public string Uid { get; set; }
    public bool IsVerified { get; set; }
    public string Ip { get; set; }

    public Player()
    {
        
    }
    
    [JsonConstructor]
    public Player(string guid, string name, string uid, bool isVerified, string ip)
    {
        Guid = guid.ToLower();
        Name = name;
        Uid = uid;
        IsVerified = isVerified;
        Ip = ip;
    }
}