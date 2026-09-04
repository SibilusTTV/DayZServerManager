
namespace Domain.Mission.RarityFile;

public class RarityFile
{
    public List<RarityItem> ItemRarity { get; set; }

    public RarityFile()
    {
        
    }

    public RarityFile(List<RarityItem> rarity)
    {
        ItemRarity = rarity;
    }
}
