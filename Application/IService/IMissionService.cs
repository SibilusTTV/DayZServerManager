using Domain.Mission.RarityFile;
using Domain.Mission.Types;

namespace Application.IService;

public interface IMissionService
{
    public void UpdateMission(string serverFolderName, string missionName, string missionTemplateName,
        string vanillaMissionName, string backupPath, string mapName, bool hasExpansion);

    public void UpdateTypesWithRarity(TypesFile typesFile, RarityFile rarityFile);
}