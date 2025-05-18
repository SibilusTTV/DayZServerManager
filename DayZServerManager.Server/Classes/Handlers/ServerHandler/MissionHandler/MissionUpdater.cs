using LibGit2Sharp;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EventSpawnClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.GlobalsClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesChangesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EnvironmentClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityFile;
using Microsoft.VisualBasic.FileIO;

namespace DayZServerManager.Server.Classes.Handlers.ServerHandler.MissionHandler
{
    public class MissionUpdater
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Update()
        {
            try
            {
                //Creating path variables for later use
                string missionPath = Path.Combine(Manager.MPMISSIONS_PATH, Manager.managerConfig.missionName);
                string missionTemplatePath = Path.Combine(Manager.MPMISSIONS_PATH, Manager.managerConfig.missionTemplateName);

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
                if (!Directory.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILE_NAME)))
                {
                    Directory.CreateDirectory(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILE_NAME));
                }

                // Creating example folder in CustomFiles
                List<string> customFilesDirectories = Directory.GetDirectories(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILE_NAME)).ToList();
                if (customFilesDirectories.Count == 0)
                {
                    Directory.CreateDirectory(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILE_NAME, Manager.MISSION_EXAMPLE_MOD_FILES_FOLDER_NAME));
                    customFilesDirectories = Directory.GetDirectories(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILE_NAME)).ToList();
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
                    XMLSerializer.SerializeXMLFile(Path.Combine(customFilesDirectories[0], Manager.MISSION_EXAMPLE_TYPES_FILE_NAME), exampleTypesFile);
                }

                // Creating Exmple cfgeconomycore
                if (!File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_ECONOMYCORE_FILE_NAME)))
                {
                    EconomyCoreFile exampleEconomyCore = new EconomyCoreFile()
                    {
                        ceItems = new List<CeItem>()
                    {
                        new CeItem()
                        {
                            folder = Path.Combine(Manager.MISSION_CUSTOM_FILE_NAME, Path.GetFileName(customFilesDirectories[0])),
                            fileItems = new List<FileItem>()
                            {
                                new FileItem()
                                {
                                    name = Manager.MISSION_EXAMPLE_TYPES_FILE_NAME,
                                    type = "types"
                                }
                            }
                        }
                    }
                    };
                    XMLSerializer.SerializeXMLFile(Path.Combine(missionTemplatePath, Manager.MISSION_ECONOMYCORE_FILE_NAME), exampleEconomyCore);
                }
                else
                {
                    EconomyCoreFile? economyCoreFile = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, Manager.MISSION_ECONOMYCORE_FILE_NAME));
                    if (economyCoreFile != null && economyCoreFile.ceItems == null)
                    {
                        economyCoreFile.ceItems = new List<CeItem>()
                    {
                        new CeItem()
                        {
                            folder = Path.Combine(Manager.MISSION_CUSTOM_FILE_NAME, Path.GetFileName(customFilesDirectories[0])),
                            fileItems = new List<FileItem>()
                            {
                                new FileItem()
                                {
                                    name = Manager.MISSION_EXAMPLE_TYPES_FILE_NAME,
                                    type = "types"
                                }
                            }
                        }
                    };
                        XMLSerializer.SerializeXMLFile(Path.Combine(missionTemplatePath, Manager.MISSION_ECONOMYCORE_FILE_NAME), economyCoreFile);
                    }
                }
                #endregion Create example CustomFiles

                #region Creating example rarities and types changes files
                //Creating customFilesRarities.json file
                if (!File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILES_RARITIES_FILE_NAME)))
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
                    JSONSerializer.SerializeJSONFile(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILES_RARITIES_FILE_NAME), customFilesRarities);
                }

                //Creating vanillaRarities.json
                if (!File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_VANILLA_RARITIES_FILE_NAME)))
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
                    JSONSerializer.SerializeJSONFile(Path.Combine(missionTemplatePath, Manager.MISSION_VANILLA_RARITIES_FILE_NAME), vanillaRarities);
                }

                //Creating vanillaTypesChanges.json
                if (!File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_VANILLA_TYPES_CHANGES_FILE_NAME)))
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

                    JSONSerializer.SerializeJSONFile(Path.Combine(missionTemplatePath, Manager.MISSION_VANILLA_TYPES_CHANGES_FILE_NAME), vanillaTypesChanges);
                }

                //Creating expansionRarities.json and expansionTypesChanges.json, if Expansion is part of the mods
                if (Manager.managerConfig.clientMods != null && Manager.managerConfig.clientMods.FindAll(p => p.name.ToLower().Contains("expansion")).Count > 0)
                {
                    //Creating expansionRarities.json
                    if (!File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_RARITIES_FILE_NAME)))
                    {
                        RarityFile expansionRarityFile = new RarityFile();
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
                        JSONSerializer.SerializeJSONFile(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_RARITIES_FILE_NAME), expansionRarityFile);
                    }

                    //Creating expansionTypesChanges.json
                    if (!File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_TYPES_CHANGES_FILE_NAME)))
                    {
                        TypesChangesFile expansionTypesChanges = new TypesChangesFile();
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

                        JSONSerializer.SerializeJSONFile(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_TYPES_CHANGES_FILE_NAME), expansionTypesChanges);
                    }

                    // Creating expansion folder in the missionTemplate folder, if it doesn't exist
                    if (!Directory.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_FOLDER_NAME)))
                    {
                        Directory.CreateDirectory(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_FOLDER_NAME));
                    }

                    // Creating settings folder in the expansion folder of the missionTemplate folder, if it doesn't exist
                    if (!Directory.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME)))
                    {
                        Directory.CreateDirectory(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME));
                    }

                    // Creating HardlineSettings.json in the settings folder of the expansion folder of the missionTemplate folder, if it doesn't exist
                    if (!File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME, Manager.MISSION_EXPANSION_HARDLINE_SETTINGS_FILE_NAME)))
                    {
                        HardlineFile? exampleHardlineRarity = new HardlineFile
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

                        JSONSerializer.SerializeJSONFile(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME, Manager.MISSION_EXPANSION_HARDLINE_SETTINGS_FILE_NAME), exampleHardlineRarity);
                    }
                }
                #endregion Creating example rarities and types changes files

                // Rename the old mission folder and copy the contents of the vanilla folder
                CopyVanillaMissionFolder(missionPath, Path.Combine(Manager.MPMISSIONS_PATH, Manager.managerConfig.vanillaMissionName), Manager.managerConfig.backupPath);

                string expansionTemplatePath = Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", Manager.managerConfig.mapName);
                if (Manager.managerConfig.clientMods != null && Manager.managerConfig.clientMods.FindAll(p => p.name.ToLower().Contains(Manager.EXPANSION_MOD_SEARCH)).Count > 0)
                {
                    // Get the new expansion mission template from git
                    expansionTemplatePath = DownloadExpansionTemplates();

                    // Copy the folder expansion_ce from the expansionTemplate to the new mission folder
                    CopyExpansionTemplateFiles(expansionTemplatePath, missionPath, missionPath + "Old");
                }

                // Copy the folders CustomFiles and expansion and also the files mapgrouppos.xml, cfgweather.xml and cfgplayerspawnpoints.xml from the missionTemplate to the new mission folder
                CopyMissionTemplateFiles(missionTemplatePath, missionPath);

                if (Directory.Exists(missionPath))
                {
                    if (File.Exists(Path.Combine(missionPath, Manager.MISSION_DB_FOLDER_NAME, Manager.MISSION_GLOBALS_FILE_NAME)))
                    {
                        // Change the variables in the globals.xml of TimeLogin to 5 and ZombieMaxCount to 500
                        GlobalsFile? globals = XMLSerializer.DeserializeXMLFile<GlobalsFile>(Path.Combine(missionPath, Manager.MISSION_DB_FOLDER_NAME, Manager.MISSION_GLOBALS_FILE_NAME));
                        if (globals != null)
                        {
                            UpdateGlobals(globals);
                            XMLSerializer.SerializeXMLFile(Path.Combine(missionPath, Manager.MISSION_DB_FOLDER_NAME, Manager.MISSION_GLOBALS_FILE_NAME), globals);
                        }
                    }

                    // Add the other parts of the cfgeconomycore.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EconomyCoreFile? missionEconomyCore = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(missionPath, Manager.MISSION_ECONOMYCORE_FILE_NAME));

                    if (missionEconomyCore != null)
                    {
                        EconomyCoreFile? expansionTemplateEconomyCore = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(expansionTemplatePath, Manager.MISSION_ECONOMYCORE_FILE_NAME));
                        EconomyCoreFile? missionTemplateEconomyCore = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, Manager.MISSION_ECONOMYCORE_FILE_NAME));
                        if (expansionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, expansionTemplateEconomyCore);
                        }
                        if (missionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, missionTemplateEconomyCore);
                        }
                        XMLSerializer.SerializeXMLFile(Path.Combine(missionPath, Manager.MISSION_ECONOMYCORE_FILE_NAME), missionEconomyCore);
                    }

                    // Add the other parts of the cfgeventspawns.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EventSpawnsFile? missionEventSpawns = XMLSerializer.DeserializeXMLFile<EventSpawnsFile>(Path.Combine(missionPath, Manager.MISSION_EVENTSPAWNS_FILE_NAME));

                    if (missionEventSpawns != null)
                    {
                        EventSpawnsFile? expansionTemplateEventSpawns = XMLSerializer.DeserializeXMLFile<EventSpawnsFile>(Path.Combine(expansionTemplatePath, Manager.MISSION_EVENTSPAWNS_FILE_NAME));
                        EventSpawnsFile? missionTemplateEventSpawns = XMLSerializer.DeserializeXMLFile<EventSpawnsFile>(Path.Combine(missionTemplatePath, Manager.MISSION_EVENTSPAWNS_FILE_NAME));
                        if (expansionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, expansionTemplateEventSpawns);
                        }
                        if (missionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, missionTemplateEventSpawns);
                        }
                        XMLSerializer.SerializeXMLFile(Path.Combine(missionPath, Manager.MISSION_EVENTSPAWNS_FILE_NAME), missionEventSpawns);
                    }

                    EnvironmentFile? missionEnvironmentFile = XMLSerializer.DeserializeXMLFile<EnvironmentFile>(Path.Combine(missionPath, Manager.MISSION_ENVIRONMENTS_FILE_NAME));

                    if (missionEnvironmentFile != null)
                    {
                        EnvironmentFile? expansionTemplateEnvironmentFile = XMLSerializer.DeserializeXMLFile<EnvironmentFile>(Path.Combine(expansionTemplatePath, Manager.MISSION_ENVIRONMENTS_FILE_NAME));
                        EnvironmentFile? missionTemplateEnvironmentFile = XMLSerializer.DeserializeXMLFile<EnvironmentFile>(Path.Combine(missionTemplatePath, Manager.MISSION_ENVIRONMENTS_FILE_NAME));

                        if (expansionTemplateEnvironmentFile != null)
                        {
                            UpdateEnvironmentFile(missionEnvironmentFile, expansionTemplateEnvironmentFile);
                        }
                        if (missionTemplateEnvironmentFile != null)
                        {
                            UpdateEnvironmentFile(missionEnvironmentFile, missionTemplateEnvironmentFile);
                        }
                        XMLSerializer.SerializeXMLFile(Path.Combine(missionPath, Manager.MISSION_ENVIRONMENTS_FILE_NAME), missionEnvironmentFile);
                    }

                    // Add the part of the main method of the init.c of the missionTemplate to the one from the new mission folder
                    if (File.Exists(Path.Combine(missionPath, Manager.MISSION_INIT_FILE_NAME)) && File.Exists(Path.Combine(missionTemplatePath, Manager.MISSION_INIT_FILE_NAME)))
                    {
                        string missionInit = InitFileSerializer.DeserializeInitFile(Path.Combine(missionPath, Manager.MISSION_INIT_FILE_NAME));
                        string templateInit = InitFileSerializer.DeserializeInitFile(Path.Combine(missionTemplatePath, Manager.MISSION_INIT_FILE_NAME));

                        missionInit = UpdateInit(missionInit, templateInit);

                        InitFileSerializer.SerializeInitFile(Path.Combine(missionPath, Manager.MISSION_INIT_FILE_NAME), missionInit);
                    }

                    // Changing the types files to reflect the rarities
                    HardlineFile? hardlineFile = JSONSerializer.DeserializeJSONFile<HardlineFile>(Path.Combine(missionPath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME, Manager.MISSION_EXPANSION_HARDLINE_SETTINGS_FILE_NAME));
                    RarityFile? vanillaRarity = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, Manager.MISSION_VANILLA_RARITIES_FILE_NAME));
                    RarityFile? expansionRarity = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_RARITIES_FILE_NAME));

                    TypesFile? vanillaTypes = XMLSerializer.DeserializeXMLFile<TypesFile>(Path.Combine(missionPath, Manager.MISSION_DB_FOLDER_NAME, Manager.MISSION_TYPES_FILE_NAME));
                    TypesFile? expansionTypes = XMLSerializer.DeserializeXMLFile<TypesFile>(Path.Combine(missionPath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME, Manager.MISSION_EXPANSION_TYPES_FILE_NAME));

                    if (hardlineFile != null)
                    {
                        RarityFile? customFilesRarityFile = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILES_RARITIES_FILE_NAME));
                        if (vanillaRarity != null)
                        {
                            UpdateHardlineRarity(hardlineFile, vanillaRarity);
                        }
                        if (expansionRarity != null)
                        {
                            UpdateHardlineRarity(hardlineFile, expansionRarity);
                        }
                        if (customFilesRarityFile != null)
                        {
                            UpdateHardlineRarity(hardlineFile, customFilesRarityFile);
                        }
                        JSONSerializer.SerializeJSONFile(Path.Combine(missionPath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME, Manager.MISSION_EXPANSION_HARDLINE_SETTINGS_FILE_NAME), hardlineFile);
                    }

                    if (vanillaTypes != null)
                    {
                        if (vanillaRarity != null)
                        {
                            UpdateTypesWithRarity(vanillaTypes, vanillaRarity);
                        }

                        // Change the Lifetimes of items in the types.xml
                        TypesChangesFile? changes = JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(missionTemplatePath, Manager.MISSION_VANILLA_TYPES_CHANGES_FILE_NAME));
                        if (changes != null)
                        {
                            UpdateTypesWithTypesChanges(vanillaTypes, changes);
                        }

                        XMLSerializer.SerializeXMLFile(Path.Combine(missionPath, Manager.MISSION_DB_FOLDER_NAME, Manager.MISSION_TYPES_FILE_NAME), vanillaTypes);
                    }

                    if (expansionTypes != null)
                    {
                        if (expansionRarity != null)
                        {
                            UpdateTypesWithRarity(expansionTypes, expansionRarity);
                        }

                        // Change the Lifetimes of items in the expansionTypes.xml
                        TypesChangesFile? changes = JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_TYPES_CHANGES_FILE_NAME));
                        if (changes != null)
                        {
                            UpdateTypesWithTypesChanges(expansionTypes, changes);
                        }

                        XMLSerializer.SerializeXMLFile(Path.Combine(missionPath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME, Manager.MISSION_EXPANSION_TYPES_FILE_NAME), expansionTypes);
                    }
                }

                if (Directory.Exists(missionPath + "Old"))
                {

                    // Copy over the data and map from the old mission into the new one
                    if (Directory.Exists(Path.Combine(missionPath + "Old", Manager.PERSISTANCE_FOLDER_NAME)))
                    {
                        CopyPersistenceData(missionPath, missionPath + "Old");
                    }

                    // Move old mission to backup
                    MoveOldMission(Path.Combine(missionPath + "Old"), Manager.managerConfig.backupPath);

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating the mission");
            }
        }

        #region Searches
        // Searches for the matching CeItem and returns it
        private static CeItem? SearchForCeItem(CeItem ceItem, EconomyCoreFile cfg)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when searching for CeItem");
                return null;
            }
        }

        // Searches for the matching FileItem and returns true, if it finds smth
        private static bool SearchForFileItem(FileItem fileItem, CeItem ceItem)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when searching for FileItem");
                return false;
            }
        }

        // Searches for the matching EventItem and returns it
        private static EventItem? SearchForEventItem(EventItem eventItem, EventSpawnsFile cfg)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when searching for EventItem");
                return null;
            }
        }

        // Searches for the matching PosItem and returns true, if it finds it
        private static bool SearchForPosItem(PosItem posItem, EventItem eventItem)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when searching for PosItem");
                return false;
            }
        }

        // Searches for the matching TypesItem and returns it
        private static TypesItem? SearchForTypesItem(string name, TypesFile typesFile)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when searching for TypesItem");
                return null;
            }
        }

        private static string SearchForExpansionTemplate(string folderPath)
        {
            try
            {
                List<string> folderDirectories = Directory.GetDirectories(folderPath).ToList();
                foreach (string folder in folderDirectories)
                {
                    if (Path.GetFileName(folder).ToLower() == Manager.managerConfig.mapName.ToLower())
                    {
                        return folder;
                    }
                }
                foreach (string folder in folderDirectories)
                {
                    string matchingTemplateFolder = SearchForExpansionTemplate(folder);
                    if (matchingTemplateFolder != "")
                    {
                        return matchingTemplateFolder;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when searching for the expansion template");
            }
            return string.Empty;
        }
        #endregion Searches

        #region UpdateFunctions
        private static string UpdateInit(string init, string templateInit)
        {
            try
            {
                Logger.Info("Updating init");
                int initStartIndex = init.IndexOf("{") + 1;
                int templateStartIndex = templateInit.IndexOf("{") + 1;
                int templateEndIndex = templateInit.LastIndexOf("}") - 1;
                int templateLength = templateEndIndex - templateStartIndex;
                if (templateLength > 0)
                {
                    string insertionString = templateInit.Substring(templateStartIndex, templateLength);
                    Logger.Info("Finished updating init");
                    return init.Insert(initStartIndex, insertionString);
                }
                else
                {
                    Logger.Info("Finished updating init");
                    return init;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating init");
                return init;
            }
        }

        private static void UpdateGlobals(GlobalsFile globals)
        {
            try
            {
                Logger.Info("Updating globals");
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
                Logger.Info("Finished updating globals");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating globals");
            }
        }

        // Updates the Rarity in the given RarityFile with the rarities of another RarityFile
        public static void UpdateHardlineRarity(HardlineFile hardlineFile, RarityFile newRarities)
        {
            try
            {
                if (newRarities.ItemRarity != null && hardlineFile.ItemRarity != null)
                {
                    Logger.Info("Added rarities to hardline file");
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
                    Logger.Info("Finished adding rarities to hardline file");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating hardline rarity");
            }
        }

        // Updates the spawning of items in the given TypesFile with the new spawns of another TypesFile
        public static void UpdateTypesWithRarity(TypesFile typesFile, RarityFile rarityFile)
        {
            try
            {
                if (rarityFile.ItemRarity != null)
                {
                    Logger.Info("Updating types with rarity");
                    foreach (RarityItem rarityitem in rarityFile.ItemRarity)
                    {
                        TypesItem? item = SearchForTypesItem(rarityitem.name, typesFile);
                        if (item != null)
                        {
                            switch (rarityitem.rarity)
                            {
                                case 0:
                                    item.nominal = 0;
                                    item.min = 0;
                                    break;
                                case 1:
                                    item.nominal = Manager.POOR_NOMINAL;
                                    item.min = Manager.POOR_MINIMAL;
                                    break;
                                case 2:
                                    item.nominal = Manager.COMMON_NOMINAL;
                                    item.min = Manager.COMMON_MINIMAL;
                                    break;
                                case 3:
                                    item.nominal = Manager.UNCOMMON_NOMINAL;
                                    item.min = Manager.UNCOMMON_MINIMAL;
                                    break;
                                case 4:
                                    item.nominal = Manager.RARE_NOMINAL;
                                    item.min = Manager.RARE_MINIMAL;
                                    break;
                                case 5:
                                    item.nominal = Manager.EPIC_NOMINAL;
                                    item.min = Manager.EPIC_MINIMAL;
                                    break;
                                case 6:
                                    item.nominal = Manager.LEGENDARY_NOMINAL;
                                    item.min = Manager.LEGENDARY_MINIMAL;
                                    break;
                                case 7:
                                    item.nominal = Manager.MYTHIC_NOMINAL;
                                    item.min = Manager.MYTHIC_MINIMAL;
                                    break;
                                case 8:
                                    item.nominal = Manager.EXOTIC_NOMINAL;
                                    item.min = Manager.EXOTIC_MINIMAL;
                                    break;
                            }
                        }
                    }
                    Logger.Info("Finished updating types with rarity");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating types with rarity");
            }
        }

        // Updates the lifetime of items in the given TypesFile with the new spawns of another TypesFile
        public static void UpdateTypesWithTypesChanges(TypesFile typesFile, TypesChangesFile changesFile)
        {
            try
            {
                Logger.Info("Updating lifetimes");
                foreach (TypesChangesItem change in changesFile.types)
                {
                    TypesItem? item = SearchForTypesItem(change.name, typesFile);
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
                Logger.Info("Finished updating lifetimes");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating lifetimes");
            }
        }

        private static void UpdateEventSpawns(EventSpawnsFile missionEventSpawns, EventSpawnsFile templateEventSpawns)
        {
            try
            {
                Logger.Info("Updating event spawns");
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
                Logger.Info("Finished updating event spawns");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating EventSpawns");
            }
        }

        private static void UpdateEnvironmentFile(EnvironmentFile missionEnvironmentFile, EnvironmentFile tempalteEnvironmentFile)
        {
            try
            {
                Logger.Info("Updating environment file");
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
                Logger.Info("Finished updating environment file");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating EnvironmentFile");
            }
        }

        private static void UpdateEconomyCore(EconomyCoreFile economyCoreFile, EconomyCoreFile templateEconomyCoreFile)
        {
            try
            {
                Logger.Info("Updating economy core");
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
                Logger.Info("Finsihed updating Economy Core");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating EconomyCore");
            }
        }
        #endregion UpdateFunctions

        #region CopyFunctions
        private static void CopyVanillaMissionFolder(string missionPath, string vanillaMissionPath, string backupPath)
        {
            try
            {
                Logger.Info("Copying vanilla mission files and folders");
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
                Logger.Info("Finished copying vanilla mission files and folders");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when copying vanilla mission folder");
            }
        }

        private static void CopyExpansionTemplateFiles(string expansionTemplatePath, string missionPath, string oldMissionPath)
        {
            try
            {
                Logger.Info("Copying expansion template files");
                if (Directory.Exists(Path.Combine(expansionTemplatePath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME)))
                {
                    FileSystem.CopyDirectory(Path.Combine(expansionTemplatePath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME), Path.Combine(missionPath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME), true);
                }
                else if (Directory.Exists(Path.Combine(oldMissionPath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME)))
                {
                    FileSystem.CopyDirectory(Path.Combine(oldMissionPath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME), Path.Combine(missionPath, Manager.MISSION_EXPANSIONCE_FOLDER_NAME), true);
                }
                Logger.Info("Finished copying expansion template files");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when copying expansion template files");
            }
        }

        private static void CopyMissionTemplateFiles(string missionTemplatePath, string missionPath)
        {
            try
            {
                Logger.Info("Moving mission template files and folders");
                List<string> templateDirectories = Directory.GetDirectories(missionTemplatePath).ToList();
                List<string> templateFiles = Directory.GetFiles(missionTemplatePath).ToList();

                foreach (string directory in templateDirectories)
                {
                    FileSystem.CopyDirectory(directory, Path.Combine(missionPath, Path.GetFileName(directory)), true);
                }

                foreach (string file in templateFiles)
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName != Manager.MISSION_ECONOMYCORE_FILE_NAME
                        && fileName != Manager.MISSION_EVENTSPAWNS_FILE_NAME
                        && fileName != Manager.MISSION_ENVIRONMENTS_FILE_NAME
                        && fileName != Manager.MISSION_CUSTOM_FILES_RARITIES_FILE_NAME
                        && fileName != Manager.MISSION_EXPANSION_RARITIES_FILE_NAME
                        && fileName != Manager.MISSION_VANILLA_RARITIES_FILE_NAME
                        && fileName != Manager.MISSION_INIT_FILE_NAME
                        && fileName != Manager.MISSION_VANILLA_RARITIES_FILE_NAME
                        && fileName != Manager.MISSION_VANILLA_TYPES_CHANGES_FILE_NAME)
                    {
                        File.Copy(file, Path.Combine(missionPath, Path.GetFileName(file)), true);
                    }
                }
                Logger.Info("Finshed moving mission template files and folders");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when copying mission template files and folders");
            }
        }

        private static void CopyPersistenceData(string missionPath, string oldMissionPath)
        {
            try
            {
                Logger.Info("Copying old persistance data");
                FileSystem.CopyDirectory(Path.Combine(oldMissionPath, Manager.PERSISTANCE_FOLDER_NAME), Path.Combine(missionPath, Manager.PERSISTANCE_FOLDER_NAME), true);
                Logger.Info("Finished copy old persistance data");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when copying persistance data");
            }
        }

        private static void MoveOldMission(string oldPath, string backupPath)
        {
            try
            {
                Logger.Info("Moving old mission");
                string newPath = Path.Combine(backupPath, Manager.BACKUPS_FULL_MISSION_BACKUPS_FOLDER_NAME, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                if (!Directory.Exists(Path.Combine(backupPath, Manager.BACKUPS_FULL_MISSION_BACKUPS_FOLDER_NAME)))
                {
                    Directory.CreateDirectory(Path.Combine(backupPath, Manager.BACKUPS_FULL_MISSION_BACKUPS_FOLDER_NAME));
                }
                Directory.Move(oldPath, newPath);
                Logger.Info("Finished moving old mission");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when moving the old mission");
            }
        }
        #endregion CopyFunctions

        #region DownloadFunctions
        private static string DownloadExpansionTemplates()
        {
            try
            {
                Logger.Info("Downloading expansion template");
                if (Directory.Exists(Manager.EXPANSION_DOWNLOAD_PATH))
                {
                    Repository rep = new Repository(Manager.EXPANSION_DOWNLOAD_PATH);
                    PullOptions pullOptions = new PullOptions();
                    pullOptions.FetchOptions = new FetchOptions();
                    Commands.Pull(rep, new Signature("username", "email", new DateTimeOffset(DateTime.Now)), pullOptions);

                    Logger.Info("Finished downloading expansion template");
                    return SearchForExpansionTemplate(Manager.EXPANSION_DOWNLOAD_PATH);
                }
                else
                {
                    Repository.Clone("https://github.com/ExpansionModTeam/DayZ-Expansion-Missions.git", Manager.EXPANSION_DOWNLOAD_PATH);

                    Logger.Info("Finished downloading expansion template");
                    return SearchForExpansionTemplate(Manager.EXPANSION_DOWNLOAD_PATH);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when downloading the expansion template");
            }
            return string.Empty;
        }
        #endregion DownloadFunctions
    }
}
