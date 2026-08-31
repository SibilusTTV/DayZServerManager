using Domain.Mission.RarityFile;

namespace Application.IRepository;

public interface IRarityRepository
{
    public RarityFile? GetRarityFile(string name, string missionTemplateName, string serverFolderPath);
    public void UpdateRarityFile(string missionFolder, string name, RarityFile rarityFile);
}