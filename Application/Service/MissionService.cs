using Application.IRepository;
using Application.IService;
using Domain.Constants;
using Domain.Mission.EconomyCore;
using Domain.Mission.Environment;
using Domain.Mission.EventSpawn;
using Domain.Mission.Globals;
using Domain.Mission.Hardline;
using Domain.Mission.RarityFile;
using Domain.Mission.Types;
using Domain.Mission.TypesChanges;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class MissionService : IMissionService
{
    private readonly ILogger<MissionService> _logger;
    private readonly IMissionRepository _missionRepository;

    public MissionService(ILogger<MissionService> logger, IMissionRepository missionRepository)
    {
        _logger = logger;
        _missionRepository = missionRepository;
    }
    
    public void UpdateMission(string serverFolderName, string missionName, string missionTemplateName, string vanillaMissionName, string backupPath, string mapName, bool hasExpansion)
    {
        try
        {
            //Creating path variables for later use
            var missionPath = Path.Combine(serverFolderName, Folders.MpmissionsFolderName, missionName);
            var missionTemplatePath = Path.Combine(serverFolderName, Folders.MpmissionsFolderName, missionTemplateName);
            
            var hardlineFile = _missionRepository.GetJsonFile<HardlineFile>(Path.Combine(missionPath, Folders.MissionExpansionFolderName, Folders.MissionExpansionSettingsFolderName, Files.MissionExpansionHardlineSettingsFileName));

            var vanillaRarity =
                _missionRepository.GetJsonFile<RarityFile>(Path.Combine(missionTemplatePath,
                    Files.MissionVanillaRaritiesFileName)) ?? new RarityFile([]);
                
            var expansionRarity =
                _missionRepository.GetJsonFile<RarityFile>(Path.Combine(missionTemplatePath,
                    Files.MissionExpansionRaritiesFileName)) ?? new RarityFile([]);
                
            var customFilesRarityFile =
                _missionRepository.GetJsonFile<RarityFile>(Path.Combine(missionTemplatePath,
                    Files.MissionCustomFilesRaritiesFileName)) ?? new RarityFile([]);
            
            _missionRepository.CreateDirectoriesAndFolders(missionPath, missionTemplatePath, hasExpansion);

            // Rename the old mission folder and copy the contents of the vanilla folder
            _missionRepository.CopyVanillaMissionFolder(missionPath, Path.Combine(serverFolderName, Folders.MpmissionsFolderName, vanillaMissionName), backupPath);

            string expansionTemplatePath = Path.Combine(Folders.ExpansionDownloadFolderPath, "Template", mapName);
            if (hasExpansion)
            {
                // Get the new expansion mission template from git
                expansionTemplatePath = _missionRepository.DownloadExpansionTemplates(mapName);

                // Copy the folder expansion_ce from the expansionTemplate to the new mission folder
                _missionRepository.CopyExpansionTemplateFiles(expansionTemplatePath, missionPath, missionPath + "Old");
            }

            // Copy the folders CustomFiles and expansion and also the files mapgrouppos.xml, cfgweather.xml and cfgplayerspawnpoints.xml from the missionTemplate to the new mission folder
            _missionRepository.CopyMissionTemplateFiles(missionTemplatePath, missionPath);

            if (Directory.Exists(missionPath))
            {
                if (File.Exists(Path.Combine(missionPath, Folders.MissionDbFolderName, Files.MissionGlobalsFileName)))
                {
                    // Change the variables in the globals.xml of TimeLogin to 5 and ZombieMaxCount to 500
                    var globals = _missionRepository.GetXmlFile<GlobalsFile>(Path.Combine(missionPath, Folders.MissionDbFolderName, Files.MissionGlobalsFileName));
                    if (globals != null)
                    {
                        UpdateGlobals(globals);
                        _missionRepository.SaveXmlFile(Path.Combine(missionPath, Folders.MissionDbFolderName, Files.MissionGlobalsFileName), globals);
                    }
                }

                // Add the other parts of the cfgeconomycore.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                EconomyCoreFile? missionEconomyCore = GetEconomyCoreFile(missionPath);

                if (missionEconomyCore != null)
                {
                    EconomyCoreFile? expansionTemplateEconomyCore = _missionRepository.GetXmlFile<EconomyCoreFile>(Path.Combine(expansionTemplatePath, Files.MissionEconomyCoreFileName));
                    EconomyCoreFile? missionTemplateEconomyCore = _missionRepository.GetXmlFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, Files.MissionEconomyCoreFileName));
                    if (expansionTemplateEconomyCore != null)
                    {
                        UpdateEconomyCore(missionEconomyCore, expansionTemplateEconomyCore);
                    }
                    if (missionTemplateEconomyCore != null)
                    {
                        UpdateEconomyCore(missionEconomyCore, missionTemplateEconomyCore);
                    }
                    _missionRepository.SaveXmlFile(Path.Combine(missionPath, Files.MissionEconomyCoreFileName), missionEconomyCore);
                    
                    UpdateCustomFilesRarities(missionEconomyCore, customFilesRarityFile, missionTemplatePath);
                    _missionRepository.SaveJsonFile(
                        Path.Combine(missionTemplatePath, Files.MissionCustomFilesRaritiesFileName), customFilesRarityFile);

                    if (hardlineFile != null)
                    {
                        UpdateHardlineRarity(hardlineFile, customFilesRarityFile);
                    }
                }

                // Add the other parts of the cfgeventspawns.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                EventSpawnsFile? missionEventSpawns = _missionRepository.GetXmlFile<EventSpawnsFile>(Path.Combine(missionPath, Files.MissionEventSpawnsFileName));

                if (missionEventSpawns != null)
                {
                    EventSpawnsFile? expansionTemplateEventSpawns = _missionRepository.GetXmlFile<EventSpawnsFile>(Path.Combine(expansionTemplatePath, Files.MissionEventSpawnsFileName));
                    EventSpawnsFile? missionTemplateEventSpawns = _missionRepository.GetXmlFile<EventSpawnsFile>(Path.Combine(missionTemplatePath, Files.MissionEventSpawnsFileName));
                    if (expansionTemplateEventSpawns != null)
                    {
                        UpdateEventSpawns(missionEventSpawns, expansionTemplateEventSpawns);
                    }
                    if (missionTemplateEventSpawns != null)
                    {
                        UpdateEventSpawns(missionEventSpawns, missionTemplateEventSpawns);
                    }
                    _missionRepository.SaveXmlFile(Path.Combine(missionPath, Files.MissionEventSpawnsFileName), missionEventSpawns);
                }

                EnvironmentFile? missionEnvironmentFile = _missionRepository.GetXmlFile<EnvironmentFile>(Path.Combine(missionPath, Files.MissionEnvironmentsFileName));

                if (missionEnvironmentFile != null)
                {
                    EnvironmentFile? expansionTemplateEnvironmentFile = _missionRepository.GetXmlFile<EnvironmentFile>(Path.Combine(expansionTemplatePath, Files.MissionEnvironmentsFileName));
                    EnvironmentFile? missionTemplateEnvironmentFile = _missionRepository.GetXmlFile<EnvironmentFile>(Path.Combine(missionTemplatePath, Files.MissionEnvironmentsFileName));

                    if (expansionTemplateEnvironmentFile != null)
                    {
                        UpdateEnvironmentFile(missionEnvironmentFile, expansionTemplateEnvironmentFile);
                    }
                    if (missionTemplateEnvironmentFile != null)
                    {
                        UpdateEnvironmentFile(missionEnvironmentFile, missionTemplateEnvironmentFile);
                    }
                    _missionRepository.SaveXmlFile(Path.Combine(missionPath, Files.MissionEnvironmentsFileName), missionEnvironmentFile);
                }

                // Add the part of the main method of the init.c of the missionTemplate to the one from the new mission folder
                if (File.Exists(Path.Combine(missionPath, Files.MissionInitFileName)) && File.Exists(Path.Combine(missionTemplatePath, Files.MissionInitFileName)))
                {
                    var missionInit = _missionRepository.GetInitFile(Path.Combine(missionPath, Files.MissionInitFileName)) ?? string.Empty;
                    var templateInit = _missionRepository.GetInitFile(Path.Combine(missionTemplatePath, Files.MissionInitFileName)) ?? string.Empty;

                    missionInit = UpdateInit(missionInit, templateInit);

                    _missionRepository.SaveInitFile(Path.Combine(missionPath, Files.MissionInitFileName), missionInit);
                }

                // Changing the types files to reflect the rarities
                var vanillaTypes = _missionRepository.GetXmlFile<TypesFile>(Path.Combine(missionPath, Folders.MissionDbFolderName, Files.MissionTypesFileName));
                var expansionTypes = _missionRepository.GetXmlFile<TypesFile>(Path.Combine(missionPath, Folders.MissionExpansionCeFolderName, Files.MissionExpansionTypesFileName));

                if (vanillaTypes != null)
                {
                    UpdateVanillaRarities(vanillaTypes, vanillaRarity);
                    _missionRepository.SaveJsonFile(Path.Combine(missionTemplatePath, Files.MissionVanillaRaritiesFileName), vanillaRarity);
                    UpdateTypesWithRarity(vanillaTypes, vanillaRarity);

                    if (hardlineFile != null)
                    {
                        UpdateHardlineRarity(hardlineFile, vanillaRarity);
                    }

                    // Change the Lifetimes of items in the types.xml
                    TypesChangesFile? changes = _missionRepository.GetJsonFile<TypesChangesFile>(Path.Combine(missionTemplatePath, Files.MissionVanillaTypesChangesFileName));
                    if (changes != null)
                    {
                        UpdateTypesWithTypesChanges(vanillaTypes, changes);
                    }

                    _missionRepository.SaveXmlFile(Path.Combine(missionPath, Folders.MissionDbFolderName, Files.MissionTypesFileName), vanillaTypes);
                }

                if (expansionTypes != null)
                {
                    UpdateExpansionRarities(expansionTypes, expansionRarity);
                    _missionRepository.SaveJsonFile(Path.Combine(missionTemplatePath, Files.MissionExpansionRaritiesFileName), expansionRarity);
                    UpdateTypesWithRarity(expansionTypes, expansionRarity);
                    
                    if (hardlineFile != null)
                    {
                        UpdateHardlineRarity(hardlineFile, expansionRarity);
                    }

                    // Change the Lifetimes of items in the expansionTypes.xml
                    TypesChangesFile? changes = _missionRepository.GetJsonFile<TypesChangesFile>(Path.Combine(missionTemplatePath, Files.MissionExpansionTypesChangesFileName));
                    if (changes != null)
                    {
                        UpdateTypesWithTypesChanges(expansionTypes, changes);
                    }

                    _missionRepository.SaveXmlFile(Path.Combine(missionPath, Folders.MissionExpansionCeFolderName, Files.MissionExpansionTypesFileName), expansionTypes);
                }

                if (hardlineFile != null)
                {
                    _missionRepository.SaveJsonFile(
                        Path.Combine(missionPath, Folders.MissionExpansionFolderName,
                            Folders.MissionExpansionSettingsFolderName, Files.MissionExpansionHardlineSettingsFileName),
                        hardlineFile);
                }
            }

            if (Directory.Exists(missionPath + "Old"))
            {

                // Copy over the data and map from the old mission into the new one
                if (Directory.Exists(Path.Combine(missionPath + "Old", Folders.PersistenceFolderName)))
                {
                    _missionRepository.CopyPersistenceData(missionPath, missionPath + "Old");
                }

                // Move old mission to backup
                _missionRepository.MoveOldMission(Path.Combine(missionPath + "Old"), backupPath);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when updating the mission");
        }
    }
    
    #region Searches
    // Searches for the matching CeItem and returns it
    private CeItem? SearchForCeItem(CeItem ceItem, EconomyCoreFile cfg)
    {
        foreach (CeItem item in cfg.ceItems)
        {
            if (item.folder.ToLower().Trim() == ceItem.folder.ToLower().Trim())
            {
                return item;
            }
        }
        return null;
    }

    // Searches for the matching FileItem and returns true, if it finds smth
    private bool SearchForFileItem(FileItem fileItem, CeItem ceItem)
    {
        foreach (FileItem item in ceItem.fileItems)
        {
            if (item.name.ToLower().Trim() == fileItem.name.ToLower().Trim())
            {
                return true;
            }
        }
        return false;
    }

    // Searches for the matching EventItem and returns it
    private EventItem? SearchForEventItem(EventItem eventItem, EventSpawnsFile cfg)
    {
        foreach (EventItem item in cfg.eventItems)
        {
            if (item.name.ToLower().Trim() == eventItem.name.ToLower().Trim())
            {
                return item;
            }
        }
        return null;
    }

    // Searches for the matching PosItem and returns true, if it finds it
    private bool SearchForPosItem(PosItem posItem, EventItem eventItem)
    {
        foreach (PosItem item in eventItem.positions)
        {
            if (long.Parse(item.x) == long.Parse(posItem.x) && long.Parse(item.y) == long.Parse(posItem.y) && long.Parse(item.a) == long.Parse(posItem.a))
            {
                return true;
            }
        }
        return false;
    }

    // Searches for the matching TypesItem and returns it
    private TypesItem? SearchForTypesItem(string name, TypesFile typesFile)
    {
        foreach (TypesItem item in typesFile.typesItems)
        {
            if (item.name.ToLower().Trim() == name.ToLower().Trim())
            {
                return item;
            }
        }
        return null;
    }
    #endregion Searches
    
    #region UpdateFunctions
    private string UpdateInit(string init, string templateInit)
    {
        try
        {
            _logger.LogInformation("Updating init");
            int initStartIndex = init.IndexOf("{") + 1;
            int templateStartIndex = templateInit.IndexOf("{") + 1;
            int templateEndIndex = templateInit.LastIndexOf("}") - 1;
            int templateLength = templateEndIndex - templateStartIndex;
            if (templateLength > 0)
            {
                string insertionString = templateInit.Substring(templateStartIndex, templateLength);
                _logger.LogInformation("Finished updating init");
                return init.Insert(initStartIndex, insertionString);
            }
            else
            {
                _logger.LogInformation("Finished updating init");
                return init;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when updating init");
            return init;
        }
    }

    private void UpdateGlobals(GlobalsFile globals)
    {
        _logger.LogInformation("Updating globals");
        foreach (VarItem item in globals.varItems)
        {
            if (item != null && item.name == "TimeLogin")
            {
                item.value = "5";
            }
            else if (item != null && item.name == "ZombieMaxCount")
            {
                item.value = "500";
            }
        }
        _logger.LogInformation("Finished updating globals");
    }

    // Updates the Rarity in the given RarityFile with the rarities of another RarityFile
    private void UpdateHardlineRarity(HardlineFile hardlineFile, RarityFile newRarities)
    {
        if (hardlineFile.ItemRarity != null)
        {
            _logger.LogInformation("Added rarities to hardline file");
            foreach (RarityItem item in newRarities.ItemRarity)
            {
                if (hardlineFile.ItemRarity.ContainsKey(item.name.ToLower()))
                {
                    hardlineFile.ItemRarity[item.name.ToLower()] = item.rarity;
                }
                else if (hardlineFile.ItemRarity.ContainsKey(item.name))
                {
                    hardlineFile.ItemRarity[item.name] = item.rarity;
                }
                else
                {
                    hardlineFile.ItemRarity.Add(item.name, item.rarity);
                }
            }
            _logger.LogInformation("Finished adding rarities to hardline file");
        }
    }

    // Updates the spawning of items in the given TypesFile with the rarities of the rarityFile
    public void UpdateTypesWithRarity(TypesFile typesFile, RarityFile rarityFile)
    {
        _logger.LogInformation("Updating types with rarity");
        foreach (RarityItem rarityItem in rarityFile.ItemRarity)
        {
            TypesItem? item = SearchForTypesItem(rarityItem.name, typesFile);
            if (item != null)
            {
                switch (rarityItem.rarity)
                {
                    case 1:
                        item.nominal = Rarities.PoorNominal;
                        item.min = Rarities.PoorMinimal;
                        break;
                    case 2:
                        item.nominal = Rarities.CommonNominal;
                        item.min = Rarities.CommonMinimal;
                        break;
                    case 3:
                        item.nominal = Rarities.UncommonNominal;
                        item.min = Rarities.UncommonMinimal;
                        break;
                    case 4:
                        item.nominal = Rarities.RareNominal;
                        item.min = Rarities.RareMinimal;
                        break;
                    case 5:
                        item.nominal = Rarities.EpicNominal;
                        item.min = Rarities.EpicMinimal;
                        break;
                    case 6:
                        item.nominal = Rarities.LegendaryNominal;
                        item.min = Rarities.LegendaryMinimal;
                        break;
                    case 7:
                        item.nominal = Rarities.MythicNominal;
                        item.min = Rarities.MythicMinimal;
                        break;
                    case 8:
                        item.nominal = Rarities.ExoticNominal;
                        item.min = Rarities.ExoticMinimal;
                        break;
                    default:
                        item.nominal = 0;
                        item.min = 0;
                        break;
                }
            }
        }
        _logger.LogInformation("Finished updating types with rarity");
    }

    // Updates the lifetime of items in the given TypesFile with the new spawns of another TypesFile
    public void UpdateTypesWithTypesChanges(TypesFile typesFiles, TypesChangesFile changesFile)
    {
        _logger.LogInformation("Updating lifetimes");
        foreach (TypesChangesItem change in changesFile.types)
        {
            TypesItem? item = SearchForTypesItem(change.name, typesFiles);
            if (item != null)
            {
                if (change.nominal != null)
                {
                    item.nominal = change.nominal.Value;
                }

                if (change.lifetime != null)
                {
                    item.lifetime = change.lifetime.Value;
                }

                if (change.restock != null)
                {
                    item.restock = change.restock.Value;
                }

                if (change.min != null)
                {
                    item.min = change.min.Value;
                }

                if (change.quantmin != null)
                {
                    item.quantmin = change.quantmin.Value;
                }

                if (change.quantmax != null)
                {
                    item.quantmax = change.quantmax.Value;
                }

                if (change.cost != null)
                {
                    item.cost = change.cost.Value;
                }

                if (change.flags != null)
                {
                    item.flags.count_in_cargo = change.flags.count_in_cargo;
                    item.flags.count_in_hoarder = change.flags.count_in_hoarder;
                    item.flags.count_in_map = change.flags.count_in_map;
                    item.flags.count_in_player = change.flags.count_in_player;
                    item.flags.crafted = change.flags.crafted;
                    item.flags.deloot = change.flags.deloot;
                }

                if (change.category != null)
                {
                    item.category.name = change.category;
                }

                if (change.usage != null)
                {
                    List<UsageItem> usages = new List<UsageItem>();
                    foreach (string usageName in change.usage)
                    {
                        usages.Add(new UsageItem { name = usageName });
                    }
                    item.usage = usages;
                }

                if (change.value != null)
                {
                    List<ValueItem> values = new List<ValueItem>();
                    foreach (string valueName in change.value)
                    {
                        values.Add(new ValueItem { name = valueName });
                    }
                    item.value = values;
                }
            }
        }
        _logger.LogInformation("Finished updating lifetimes");
    }

    private void UpdateEventSpawns(EventSpawnsFile missionEventSpawns, EventSpawnsFile templateEventSpawns)
    {
        _logger.LogInformation("Updating event spawns");
        foreach (EventItem eventItem in templateEventSpawns.eventItems)
        {
            EventItem? eventItemInMission = SearchForEventItem(eventItem, missionEventSpawns);
            if (eventItemInMission != null)
            {
                foreach (PosItem posItem in eventItem.positions)
                {
                    if (!SearchForPosItem(posItem, eventItemInMission))
                    {
                        eventItemInMission.positions.Add(posItem);
                    }
                }
            }
            else
            {
                missionEventSpawns.eventItems.Add(eventItem);
            }
        }
        _logger.LogInformation("Finished updating event spawns");
    }

    private void UpdateEnvironmentFile(EnvironmentFile missionEnvironmentFile, EnvironmentFile tempalteEnvironmentFile)
    {
        _logger.LogInformation("Updating environment file");
        if (missionEnvironmentFile.Territories != null && tempalteEnvironmentFile.Territories != null)
        {
            if (missionEnvironmentFile.Territories.Files != null && tempalteEnvironmentFile.Territories.Files != null)
            {
                foreach (EnvironmentFileItem file in tempalteEnvironmentFile.Territories.Files)
                {
                    missionEnvironmentFile.Territories.Files.Add(file);
                }
            }
            if (missionEnvironmentFile.Territories.Territories != null && tempalteEnvironmentFile.Territories.Territories != null)
            {
                foreach (TerritoryItem territory in tempalteEnvironmentFile.Territories.Territories)
                {
                    missionEnvironmentFile.Territories.Territories.Add(territory);
                }
            }
        }
        _logger.LogInformation("Finished updating environment file");
    }

    private void UpdateEconomyCore(EconomyCoreFile economyCoreFile, EconomyCoreFile templateEconomyCoreFile)
    {
        _logger.LogInformation("Updating economy core");
        foreach (CeItem ceItem in templateEconomyCoreFile.ceItems)
        {
            CeItem? ceItemInMission = SearchForCeItem(ceItem, economyCoreFile);
            if (ceItemInMission != null)
            {
                foreach (FileItem fileItem in ceItem.fileItems)
                {
                    if (!SearchForFileItem(fileItem, ceItemInMission))
                    {
                        ceItemInMission.fileItems.Add(fileItem);
                    }
                }
            }
            else
            {
                economyCoreFile.ceItems.Add(ceItem);
            }
        }
        _logger.LogInformation("Finsihed updating Economy Core");
    }

    private void UpdateVanillaRarities(TypesFile typesFile, RarityFile rarityFile)
    {
        foreach (var type in typesFile.typesItems)
        {
            if (rarityFile.ItemRarity.Find(rarity => string.Equals(rarity.name, type.name, StringComparison.CurrentCultureIgnoreCase)) == null)
            {
                rarityFile.ItemRarity.Add(GetNewRarityItem(type, GetNextId(rarityFile.ItemRarity)));
            }
        }
    }

    private void UpdateExpansionRarities(TypesFile typesFile, RarityFile rarityFile)
    {
        foreach (var type in typesFile.typesItems)
        {
            if (rarityFile.ItemRarity.Find(rarity => rarity.name.ToLower() == type.name.ToLower()) == null)
            {
                rarityFile.ItemRarity.Add(GetNewRarityItem(type, GetNextId(rarityFile.ItemRarity)));
            }
        }
    }

    private void UpdateCustomFilesRarities(EconomyCoreFile economyCoreFile, RarityFile rarityFile, string folderPath)
    {
        var typesFilePaths = _missionRepository.GetAllCustomTypesFiles(economyCoreFile, folderPath);

        var filteredTypePaths = typesFilePaths.FindAll(type => !type.ToLower().Contains(SteamCmd.ExpansionModSearch));

        foreach (var typePath in filteredTypePaths)
        {
            var typesFile = _missionRepository.GetXmlFile<TypesFile>(typePath);
            
            if (typesFile == null) continue;
            
            foreach (var type in typesFile.typesItems)
            {
                if (rarityFile.ItemRarity.Find(rarity => rarity.name.ToLower() == type.name.ToLower()) == null)
                {
                    rarityFile.ItemRarity.Add(GetNewRarityItem(type, GetNextId(rarityFile.ItemRarity)));
                }
            }
        }
    }
    #endregion UpdateFunctions

    #region GetFunctions
    public EconomyCoreFile? GetEconomyCoreFile(string missionPath)
    {
        return _missionRepository.GetXmlFile<EconomyCoreFile>(Path.Combine(missionPath, Files.MissionEconomyCoreFileName));
    }
    
    private RarityItem GetNewRarityItem(TypesItem type, int id)
    {
        RarityItem newRarityItem = new RarityItem();
        newRarityItem.id = id;
        newRarityItem.name = type.name;
        
        switch (type.nominal)
        {
            case > 0 and <= Rarities.ExoticNominal:
                newRarityItem.rarity = Rarities.Exotic;
                break;
            case > Rarities.ExoticNominal and <= Rarities.MythicNominal:
                newRarityItem.rarity = Rarities.Mythic;
                break;
            case > Rarities.MythicNominal and <= Rarities.LegendaryNominal:
                newRarityItem.rarity = Rarities.Legendary;
                break;
            case > Rarities.LegendaryNominal and <= Rarities.EpicNominal:
                newRarityItem.rarity = Rarities.Epic;
                break;
            case > Rarities.EpicNominal and <= Rarities.RareNominal:
                newRarityItem.rarity = Rarities.Rare;
                break;
            case > Rarities.RareNominal and <= Rarities.UncommonNominal:
                newRarityItem.rarity = Rarities.Uncommon;
                break;
            case > Rarities.UncommonNominal and <= Rarities.CommonNominal:
                newRarityItem.rarity = Rarities.Common;
                break;
            case > Rarities.CommonNominal:
                newRarityItem.rarity = Rarities.Poor;
                break;
            default:
                newRarityItem.rarity = 0;
                break;
        }

        return newRarityItem;
    }

    private int GetNextId(List<RarityItem> items)
    {
        int nextId = 0;
        for (; nextId < items.Count; nextId++)
        {
            if (items.Find(item => item.id == nextId) == null)
            {
                return nextId;
            }
        }
        return nextId;
    }
    #endregion GetFunctions
}