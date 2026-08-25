namespace Domain.Manager;

public class Mod
{
    public Guid id { get; set; }
    public string name { get; set; }
    public long workshopID { get; set; }

    public Mod()
    {
        
    }
    
    
    public Mod(string name, long workshopID)
    {
        this.id = Guid.NewGuid();
        this.name = name;
        this.workshopID = workshopID;
    }
}