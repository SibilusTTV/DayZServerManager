using Application.IRepository;
using Domain.Constants;
using Domain.Mission.EconomyCore;
using Domain.Mission.RarityFile;
using Domain.Mission.Types;
using Domain.Mission.TypesChanges;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class RarityRepository : IRarityRepository
{
    private readonly ILogger<ServerRepository> _logger;
    private readonly IXmlSerializerRepository _xmlSerializerRepository;
    private readonly IJsonSerializerRepository _jsonSerializerRepository;

    public RarityRepository(ILogger<ServerRepository> logger, IXmlSerializerRepository xmlSerializerRepository, IJsonSerializerRepository jsonSerializerRepository)
    {
        _logger = logger;
        _xmlSerializerRepository = xmlSerializerRepository;
        _jsonSerializerRepository = jsonSerializerRepository;
    }

    #region GetFunctions
    public RarityFile? GetRarityFile(string name, string missionTemplateName, string serverFolderPath)
    {
        if (!File.Exists(Path.Combine(serverFolderPath, Folders.MpmissionsFolderName, missionTemplateName, name))) return null;
        
        return _jsonSerializerRepository.DeserializeJSONFile<RarityFile>(Path.Combine(serverFolderPath, Folders.MpmissionsFolderName, missionTemplateName, name));
    }
    #endregion GetFunctions

    #region UpdateFunctions
    public void UpdateRarityFile(string missionFolder, string name, RarityFile rarityFile)
    {
        _logger.LogInformation("Updating Rarities");
        _jsonSerializerRepository.SerializeJSONFile(Path.Combine(missionFolder, name), rarityFile);
        _logger.LogInformation("Rarities Updated");
    }
    #endregion UpdateFunctions

    #region GetTypesChanges
    private TypesChangesFile? GetExpansionTypesChangesFile(string folderPath)
    {
        if (!File.Exists(Path.Combine(folderPath, Files.MissionExpansionTypesChangesFileName))) return null;
        
        return _jsonSerializerRepository.DeserializeJSONFile<TypesChangesFile>(Path.Combine(folderPath, Files.MissionExpansionTypesChangesFileName));
    }

    private TypesChangesFile? GetVanillaTypesChangesFile(string folderPath)
    {
        if (!File.Exists(Path.Combine(folderPath, Files.MissionVanillaTypesChangesFileName))) return null;
        
        return _jsonSerializerRepository.DeserializeJSONFile<TypesChangesFile>(Path.Combine(folderPath, Files.MissionVanillaTypesChangesFileName));
    }
    #endregion GetTypesChanges
}