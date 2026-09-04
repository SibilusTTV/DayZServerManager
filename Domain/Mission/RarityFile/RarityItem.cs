
namespace Domain.Mission.RarityFile;

public class RarityItem
{
    public int id { get; set; }
    public string name { get; set; }
    public int rarity { get; set; }

    public RarityItem()
    {
        
    }

    public RarityItem(int id, string name, int rarity)
    {
        this.id = id;
        this.name = name;
        this.rarity = rarity;
    }
}