using Application.IRepository;
using Domain.Constants;
using Domain.Mission.EconomyCore;
using Domain.Mission.Environment;
using Domain.Mission.EventSpawn;
using Domain.Mission.Globals;
using Domain.Mission.Hardline;
using Domain.Mission.RarityFile;
using Domain.Mission.Types;
using Domain.Mission.TypesChanges;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;

namespace Infrastructure.Repository;

public class MissionRepository : IMissionRepository
{
    private readonly ILogger<MissionRepository> _logger;
    private readonly IJsonSerializerRepository _jsonSerializerRepository;
    private readonly IXmlSerializerRepository _xmlSerializerRepository;
    private readonly IInitFileSerializerRepository _initFileSerializerRepository;

    public MissionRepository(ILogger<MissionRepository> logger, IJsonSerializerRepository jsonSerializerRepository,
        IXmlSerializerRepository xmlSerializerRepository, IInitFileSerializerRepository initFileSerializerRepository)
    {
        _logger = logger;
        _jsonSerializerRepository = jsonSerializerRepository;
        _xmlSerializerRepository = xmlSerializerRepository;
        _initFileSerializerRepository = initFileSerializerRepository;
    }

    public void CreateDirectoriesAndFolders(string missionPath, string missionTemplatePath, bool hasExpansion)
    {
        try
        {
            #region Creating directories
            if (!Directory.Exists(missionPath))
            {
                Directory.CreateDirectory(missionPath);
            }

            if (!Directory.Exists(missionTemplatePath))
            {
                Directory.CreateDirectory(missionTemplatePath);
            }
            #endregion Creating directories

            #region Creating example CustomFiles

            //Creating CustomFiles folder
            if (!Directory.Exists(Path.Combine(missionTemplatePath, Folders.MissionCustomFilesFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(missionTemplatePath, Folders.MissionCustomFilesFolderName));
            }

            // Creating example folder in CustomFiles
            List<string> customFilesDirectories = Directory.GetDirectories(Path.Combine(missionTemplatePath, Folders.MissionCustomFilesFolderName)).ToList();
            if (customFilesDirectories.Count == 0)
            {
                Directory.CreateDirectory(Path.Combine(missionTemplatePath, Folders.MissionCustomFilesFolderName, Folders.MissionExampleModFilesFolderName));
                customFilesDirectories = Directory.GetDirectories(Path.Combine(missionTemplatePath, Folders.MissionCustomFilesFolderName)).ToList();
            }

            //Creating Example typesFile
            List<string> filesNames = Directory.GetFiles(Path.Combine(customFilesDirectories[0])).ToList();
            if (filesNames.Count == 0)
            {
                TypesFile exampleTypesFile = new TypesFile()
                {
                    typesItems = new List<TypesItem>()
                    {
                        new TypesItem()
                        {
                            name = "ExampleItem",
                            lifetime = 2000,
                            nominal = 10,
                            min = 5
                        },
                        new TypesItem()
                        {
                            name = "ExampleItem2",
                            lifetime = 20000,
                            nominal = 20,
                            min = 10
                        }
                    }
                };
                _xmlSerializerRepository.SerializeXMLFile(Path.Combine(customFilesDirectories[0], Files.MissionExampleTypesFileName), exampleTypesFile);
            }

            // Creating Exmple cfgeconomycore
            if (!File.Exists(Path.Combine(missionTemplatePath, Files.MissionEconomyCoreFileName)))
            {
                EconomyCoreFile exampleEconomyCore = new EconomyCoreFile()
                {
                    ceItems = new List<CeItem>()
                {
                    new CeItem()
                    {
                        folder = Path.Combine(Folders.MissionCustomFilesFolderName, Path.GetFileName(customFilesDirectories[0])),
                        fileItems = new List<FileItem>()
                        {
                            new FileItem()
                            {
                                name = Files.MissionExampleTypesFileName,
                                type = "types"
                            }
                        }
                    }
                }
                };
                _xmlSerializerRepository.SerializeXMLFile(Path.Combine(missionTemplatePath, Files.MissionEconomyCoreFileName), exampleEconomyCore);
            }
            else
            {
                EconomyCoreFile? economyCoreFile = _xmlSerializerRepository.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, Files.MissionEconomyCoreFileName));
                if (economyCoreFile != null)
                {
                    economyCoreFile.ceItems = new List<CeItem>()
                {
                    new CeItem()
                    {
                        folder = Path.Combine(Folders.MissionCustomFilesFolderName, Path.GetFileName(customFilesDirectories[0])),
                        fileItems = new List<FileItem>()
                        {
                            new FileItem()
                            {
                                name = Files.MissionExampleTypesFileName,
                                type = "types"
                            }
                        }
                    }
                };
                    _xmlSerializerRepository.SerializeXMLFile(Path.Combine(missionTemplatePath, Files.MissionEconomyCoreFileName), economyCoreFile);
                }
            }
            #endregion Create example CustomFiles

            #region Creating example rarities and types changes files
            //Creating customFilesRarities.json file
            if (!File.Exists(Path.Combine(missionTemplatePath, Files.MissionCustomFilesRaritiesFileName)))
            {
                RarityFile customFilesRarities = new RarityFile();
                customFilesRarities.ItemRarity = new List<RarityItem>()
                {
                    new RarityItem()
                    {
                        id = 0,
                        name = "example1",
                        rarity = 3
                    },
                    new RarityItem()
                    {
                        id = 1,
                        name = "example2",
                        rarity = 5
                    }
                };
                _jsonSerializerRepository.SerializeJSONFile(Path.Combine(missionTemplatePath, Files.MissionCustomFilesRaritiesFileName), customFilesRarities);
            }

            //Creating vanillaRarities.json
            if (!File.Exists(Path.Combine(missionTemplatePath, Files.MissionVanillaRaritiesFileName)))
            {
                RarityFile vanillaRarities = new RarityFile();
                vanillaRarities.ItemRarity = new List<RarityItem>()
                {
                    new RarityItem()
                    {
                        id = 0,
                        name = "example1",
                        rarity = 3
                    },
                    new RarityItem()
                    {
                        id = 1,
                        name = "example2",
                        rarity = 5
                    }
                };
                _jsonSerializerRepository.SerializeJSONFile(Path.Combine(missionTemplatePath, Files.MissionVanillaRaritiesFileName), vanillaRarities);
            }

            //Creating vanillaTypesChanges.json
            if (!File.Exists(Path.Combine(missionTemplatePath, Files.MissionVanillaTypesChangesFileName)))
            {
                TypesChangesFile vanillaTypesChanges = new TypesChangesFile();
                vanillaTypesChanges.types =
                [
                    new()
                    {
                        name = "example1",
                        lifetime = 3888000,
                        flags = new("0", "0", "1", "0", "0", "0"),
                        value =
                        [
                            "Tier3"
                        ]
                    },
                    new()
                    {
                        name = "example2",
                        lifetime = 3888000,
                        value = []
                    },
                    new()
                    {
                        name = "example2",
                        lifetime = 3888000
                    }
                ];

                _jsonSerializerRepository.SerializeJSONFile(Path.Combine(missionTemplatePath, Files.MissionVanillaTypesChangesFileName), vanillaTypesChanges);
            }

            //Creating expansionRarities.json and expansionTypesChanges.json, if Expansion is part of the mods
            if (hasExpansion)
            {
                //Creating expansionRarities.json
                if (!File.Exists(Path.Combine(missionTemplatePath, Files.MissionExpansionRaritiesFileName)))
                {
                    var expansionRarityFile = new RarityFile();
                    expansionRarityFile.ItemRarity = new List<RarityItem>()
                    {
                        new RarityItem()
                        {
                            id = 0,
                            name = "example1",
                            rarity = 3
                        },
                        new RarityItem()
                        {
                            id = 1,
                            name = "example2",
                            rarity = 5
                        }
                    };
                    _jsonSerializerRepository.SerializeJSONFile(Path.Combine(missionTemplatePath, Files.MissionExpansionRaritiesFileName), expansionRarityFile);
                }

                //Creating expansionTypesChanges.json
                if (!File.Exists(Path.Combine(missionTemplatePath, Files.MissionExpansionTypesChangesFileName)))
                {
                    var expansionTypesChanges = new TypesChangesFile();
                    expansionTypesChanges.types = new List<TypesChangesItem>
                    {
                        new TypesChangesItem()
                        {
                            name = "example1",
                            lifetime = 3888000
                        },
                        new TypesChangesItem()
                        {
                            name = "example2",
                            lifetime = 3888000
                        }
                    };
                    _jsonSerializerRepository.SerializeJSONFile(Path.Combine(missionTemplatePath, Files.MissionExpansionTypesChangesFileName), expansionTypesChanges);
                }

                // Creating expansion folder in the missionTemplate folder, if it doesn't exist
                if (!Directory.Exists(Path.Combine(missionTemplatePath, Folders.MissionExpansionFolderName)))
                {
                    Directory.CreateDirectory(Path.Combine(missionTemplatePath, Folders.MissionExpansionFolderName));
                }

                // Creating settings folder in the expansion folder of the missionTemplate folder, if it doesn't exist
                if (!Directory.Exists(Path.Combine(missionTemplatePath, Folders.MissionExpansionFolderName, Folders.MissionExpansionSettingsFolderName)))
                {
                    Directory.CreateDirectory(Path.Combine(missionTemplatePath, Folders.MissionExpansionFolderName, Folders.MissionExpansionSettingsFolderName));
                }

                // Creating HardlineSettings.json in the settings folder of the expansion folder of the missionTemplate folder, if it doesn't exist
                if (!File.Exists(Path.Combine(missionTemplatePath, Folders.MissionExpansionFolderName, Folders.MissionExpansionSettingsFolderName, Files.MissionExpansionHardlineSettingsFileName)))
                {
                    var exampleHardlineRarity = new HardlineFile
                    {
                        PoorItemRequirement = 0,
                        CommonItemRequirement = 0,
                        UncommonItemRequirement = 100,
                        RareItemRequirement = 200,
                        EpicItemRequirement = 400,
                        LegendaryItemRequirement = 800,
                        MythicItemRequirement = 1600,
                        ExoticItemRequirement = 3200,
                        ShowHardlineHUD = 1,
                        UseReputation = 1,
                        UseFactionReputation = 0,
                        EnableFactionPersistence = 0,
                        EnableItemRarity = 1,
                        UseItemRarityOnInventoryIcons = 1,
                        UseItemRarityForMarketPurchase = 1,
                        UseItemRarityForMarketSell = 0,
                        MaxReputation = 5000,
                        ReputationLossOnDeath = 1000,
                        DefaultItemRarity = 2,
                        EntityReputation = new Dictionary<string, int>
                        {
                            {"Animal_GallusGallusDomesticus", 1 },
                            {"eAIBase", 5 },
                            {"ZmbM_SoldierNormal_Base", 20 },
                            {"Animal_UrsusArctos", 50 },
                            {"ZmbM_NBC_Grey", 20 },
                            {"ZombieBase", 5 },
                            {"PlayerBase", 50 },
                            {"Animal_UrsusMaritimus", 50 },
                            {"ZmbM_NBC_Yellow", 20 },
                            {"AnimalBase", 1 }
                        },
                        ItemRarity = new Dictionary<string, int>()
                    };

                    _jsonSerializerRepository.SerializeJSONFile(Path.Combine(missionTemplatePath, Folders.MissionExpansionFolderName, Folders.MissionExpansionSettingsFolderName, Files.MissionExpansionHardlineSettingsFileName), exampleHardlineRarity);
                }
            }
            #endregion Creating example rarities and types changes files
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when creating directories and folders");
        }
    }

    #region Get
    public TJsonFile? GetJsonFile<TJsonFile>(string filePath)
    {
        return _jsonSerializerRepository.DeserializeJSONFile<TJsonFile>(filePath);
    }

    public TXmlFile? GetXmlFile<TXmlFile>(string filePath)
    {
        return _xmlSerializerRepository.DeserializeXMLFile<TXmlFile>(filePath);
    }
    
    public string GetInitFile(string filePath)
    {
        return _initFileSerializerRepository.DeserializeInitFile(filePath);
    }
    
    public List<string> GetAllCustomTypesFiles(string folderPath)
    {
        var typesFiles = new List<string>();
        
        if (!File.Exists(Path.Combine(folderPath, Files.MissionEconomyCoreFileName))) return typesFiles;
        
        var economyCoreFilePath = Path.Combine(folderPath, Files.MissionEconomyCoreFileName);
        var economyCoreFile = _xmlSerializerRepository.DeserializeXMLFile<EconomyCoreFile>(economyCoreFilePath);
        if (economyCoreFile == null) return typesFiles;
        
        foreach (var ceItem in economyCoreFile.ceItems)
        {
            foreach (var fileItem in ceItem.fileItems)
            {
                if (fileItem.type == "types")
                {
                    typesFiles.Add(Path.Combine(folderPath, ceItem.folder, fileItem.name));
                }
            }
        }
        return typesFiles;
    }
    #endregion Get
    
    #region SaveFunctions
    public void SaveJsonFile<TJsonFile>(string filePath, TJsonFile jsonFile)
    {
        _jsonSerializerRepository.SerializeJSONFile<TJsonFile>(filePath, jsonFile);
    }

    public void SaveXmlFile<TXmlFile>(string filePath, TXmlFile xmlFile)
    {
        _xmlSerializerRepository.SerializeXMLFile<TXmlFile>(filePath, xmlFile);
    }

    public void SaveInitFile(string filePath, string initFile)
    {
        _initFileSerializerRepository.SerializeInitFile(filePath, initFile);
    }
    #endregion
    
    #region Searches
    private string SearchForExpansionTemplate(string folderPath, string mapName)
    {
        try
        {
            var folderDirectories = Directory.GetDirectories(folderPath).ToList();
            foreach (var folder in folderDirectories)
            {
                if (string.Equals(Path.GetFileName(folder), mapName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return folder;
                }
            }
            foreach (var folder in folderDirectories)
            {
                var matchingTemplateFolder = SearchForExpansionTemplate(folder, mapName);
                if (matchingTemplateFolder != "")
                {
                    return matchingTemplateFolder;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when searching for the expansion template");
        }
        return string.Empty;
    }
    #endregion Searches

    #region CopyFunctions
    public void CopyVanillaMissionFolder(string missionPath, string vanillaMissionPath, string backupPath)
    {
        try
        {
            _logger.LogInformation("Copying vanilla mission files and folders");
            if (Directory.Exists(vanillaMissionPath))
            {
                if (Directory.Exists(missionPath))
                {
                    if (Directory.Exists(missionPath + "Old"))
                    {
                        MoveOldMission(missionPath, backupPath);
                    }
                    Directory.Move(missionPath, missionPath + "Old");
                }
                FileSystem.CopyDirectory(vanillaMissionPath, missionPath, true);
            }
            _logger.LogInformation("Finished copying vanilla mission files and folders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when copying vanilla mission folder");
        }
    }

    public void CopyExpansionTemplateFiles(string expansionTemplatePath, string missionPath, string oldMissionPath)
    {
        try
        {
            _logger.LogInformation("Copying expansion template files");
            if (Directory.Exists(Path.Combine(expansionTemplatePath, Folders.MissionExpansionCeFolderName)))
            {
                FileSystem.CopyDirectory(Path.Combine(expansionTemplatePath, Folders.MissionExpansionCeFolderName), Path.Combine(missionPath, Folders.MissionExpansionCeFolderName), true);
            }
            else if (Directory.Exists(Path.Combine(oldMissionPath, Folders.MissionExpansionCeFolderName)))
            {
                FileSystem.CopyDirectory(Path.Combine(oldMissionPath, Folders.MissionExpansionCeFolderName), Path.Combine(missionPath, Folders.MissionExpansionCeFolderName), true);
            }
            _logger.LogInformation("Finished copying expansion template files");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when copying expansion template files");
        }
    }

    public void CopyMissionTemplateFiles(string missionTemplatePath, string missionPath)
    {
        try
        {
            _logger.LogInformation("Moving mission template files and folders");
            List<string> templateDirectories = Directory.GetDirectories(missionTemplatePath).ToList();
            List<string> templateFiles = Directory.GetFiles(missionTemplatePath).ToList();

            foreach (string directory in templateDirectories)
            {
                FileSystem.CopyDirectory(directory, Path.Combine(missionPath, Path.GetFileName(directory)), true);
            }

            foreach (string file in templateFiles)
            {
                string fileName = Path.GetFileName(file);
                if (fileName != Files.MissionEconomyCoreFileName
                    && fileName != Files.MissionEventSpawnsFileName
                    && fileName != Files.MissionEnvironmentsFileName
                    && fileName != Files.MissionCustomFilesRaritiesFileName
                    && fileName != Files.MissionExpansionRaritiesFileName
                    && fileName != Files.MissionVanillaRaritiesFileName
                    && fileName != Files.MissionInitFileName
                    && fileName != Files.MissionVanillaRaritiesFileName
                    && fileName != Files.MissionVanillaTypesChangesFileName)
                {
                    File.Copy(file, Path.Combine(missionPath, Path.GetFileName(file)), true);
                }
            }
            _logger.LogInformation("Finshed moving mission template files and folders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when copying mission template files and folders");
        }
    }

    public void CopyPersistenceData(string missionPath, string oldMissionPath)
    {
        try
        {
            _logger.LogInformation("Copying old persistance data");
            FileSystem.CopyDirectory(Path.Combine(oldMissionPath, Folders.PersistenceFolderName), Path.Combine(missionPath, Folders.PersistenceFolderName), true);
            _logger.LogInformation("Finished copy old persistance data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when copying persistance data");
        }
    }

    public void MoveOldMission(string oldPath, string backupPath)
    {
        try
        {
            _logger.LogInformation("Moving old mission");
            string newPath = Path.Combine(backupPath, Folders.BackupsFullMissionBackupsFolderName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            if (!Directory.Exists(Path.Combine(backupPath, Folders.BackupsFullMissionBackupsFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(backupPath, Folders.BackupsFullMissionBackupsFolderName));
            }
            FileSystem.CopyDirectory(oldPath, newPath);
            Directory.Delete(oldPath, true);
            _logger.LogInformation("Finished moving old mission");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when moving the old mission");
        }
    }
    #endregion CopyFunctions

    #region DownloadFunctions
    public string DownloadExpansionTemplates(string mapName)
    {
        try
        {
            _logger.LogInformation("Downloading expansion template");
            if (Directory.Exists(Folders.ExpansionDownloadFolderPath))
            {
                var rep = new LibGit2Sharp.Repository(Folders.ExpansionDownloadFolderPath);
                PullOptions pullOptions = new PullOptions();
                pullOptions.FetchOptions = new FetchOptions();
                Commands.Pull(rep, new Signature("username", "email", new DateTimeOffset(DateTime.Now)), pullOptions);

                _logger.LogInformation("Finished downloading expansion template");
                return SearchForExpansionTemplate(Folders.ExpansionDownloadFolderPath, mapName);
            }
            else
            {
                LibGit2Sharp.Repository.Clone("https://github.com/ExpansionModTeam/DayZ-Expansion-Missions.git", Folders.ExpansionDownloadFolderPath);

                _logger.LogInformation("Finished downloading expansion template");
                return SearchForExpansionTemplate(Folders.ExpansionDownloadFolderPath, mapName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when downloading the expansion template");
        }
        return string.Empty;
    }
    #endregion DownloadFunction
}