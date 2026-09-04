using Domain.Mission.EconomyCore;
using Domain.Mission.RarityFile;
using Domain.Mission.Types;
using Domain.Mission.TypesChanges;

namespace Application.IService;

public interface IMissionService
{
    public void UpdateMission(string serverFolderName, string missionName, string missionTemplateName,
        string vanillaMissionName, string backupPath, string mapName, bool hasExpansion);

    public void UpdateTypesWithTypesChanges(TypesFile typesFiles, TypesChangesFile changesFile);
    public void UpdateTypesWithRarity(TypesFile typesFile, RarityFile rarityFile);

    public EconomyCoreFile? GetEconomyCoreFile(string missionPath);
}