namespace Domain.Manager;

public class Mod
{
    public string id { get; set; }
    public string name { get; set; }
    public long workshopID { get; set; }

    public Mod()
    {
        
    }
    
    
    public Mod(string name, long workshopID)
    {
        id = Guid.NewGuid().ToString().ToLower();
        this.name = name;
        this.workshopID = workshopID;
    }
}