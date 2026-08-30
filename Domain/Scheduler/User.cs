using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Scheduler;

public class User
{
    [Key]
    public string Guid { get; set; }
    public string Name { get; set; }
    public string Uid { get; set; }
    public bool IsVerified { get; set; }
    public string Ip { get; set; }

    public User()
    {
        
    }
    
    [JsonConstructor]
    public User(string guid, string name, string uid, bool isVerified, string ip)
    {
        Guid = guid.ToLower();
        Name = name;
        Uid = uid;
        IsVerified = isVerified;
        Ip = ip;
    }
}