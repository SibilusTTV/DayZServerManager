using Domain.Mission.RarityFile;

namespace Application.IService;

public interface IRarityService
{
    public RarityFile? Get(string id, string name);
    public RarityFile? Get(string name, string missionTemplateName, string serverFolderName);
    public bool UpdateRaritiesAndTypes(string id, string name, RarityFile rarityFile);

    public bool UpdateRaritiesAndTypes(string name, RarityFile rarityFile, string missionTemplateName,
        string serverFolderPath);
}