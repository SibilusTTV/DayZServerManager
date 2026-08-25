using Application.IRepository;
using Application.IService;
using Domain.Constants;
using Domain.Mission.RarityFile;
using Domain.Mission.Types;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class RarityService : IRarityService
{
    private readonly ILogger<RarityService> _logger;
    private readonly IRarityRepository _rarityRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly IMissionService _missionService;
    private readonly IInstanceService _instanceService;

    public RarityService(ILogger<RarityService> logger, IRarityRepository rarityRepository, IMissionRepository missionRepository, IMissionService missionService, IInstanceService instanceService)
    {
        _logger = logger;
        _rarityRepository = rarityRepository;
        _missionRepository = missionRepository;
        _missionService = missionService;
        _instanceService = instanceService;
    }

    public RarityFile? Get(Guid id, string name)
    {
        var instanceConfig = _instanceService.GetInstance(id);
        return instanceConfig == null ? null : Get(name, instanceConfig.missionTemplateName, instanceConfig.serverFolder);
    }

    public RarityFile? Get(string name, string missionTemplateName, string serverFolderName)
    {
        return _rarityRepository.GetRarityFile(name, missionTemplateName, serverFolderName);    
    }

    public bool UpdateRaritiesAndTypes(Guid id, string name, RarityFile rarityFile)
    {
        var instanceConfig = _instanceService.GetInstance(id);
        return instanceConfig != null && UpdateRaritiesAndTypes(name, rarityFile, instanceConfig.missionTemplateName, instanceConfig.serverFolder);
    }
    
    public bool UpdateRaritiesAndTypes(string name, RarityFile rarityFile, string missionTemplateName, string serverFolderName)
    {
        _logger.LogInformation("Updating Rarity and Types");

        _rarityRepository.UpdateRarityFile(Path.Combine(serverFolderName, Folders.MpmissionsFolderName, missionTemplateName), name, rarityFile);
        UpdateTypesFiles(Path.Combine(serverFolderName, Folders.MpmissionsFolderName), name, rarityFile, missionTemplateName);

        _logger.LogInformation("Rarity and Types updated");

        return true;
    }

    private void UpdateTypesFiles(string mpmissionsFolder, string name, RarityFile rarityFile, string missionTemplateName)
    {
        if (name != Files.MissionCustomFilesRaritiesFileName) return;
        
        _logger.LogInformation("Updating Types");
        UpdateCustomTypes(Path.Combine(mpmissionsFolder, missionTemplateName), rarityFile);
        _logger.LogInformation("Types updated");
    }

    private void UpdateCustomTypes(string folderPath, RarityFile rarityFile)
    {
        var typesFilePaths = _missionRepository.GetAllCustomTypesFiles(folderPath);
        foreach (var filePath in typesFilePaths)
        {
            var typesFile = _missionRepository.GetXmlFile<TypesFile>(filePath);
            
            if (typesFile == null) continue;
            
            _missionService.UpdateTypesWithRarity(typesFile, rarityFile);
            _missionRepository.SaveXmlFile(filePath, typesFile);
        }
    }
}