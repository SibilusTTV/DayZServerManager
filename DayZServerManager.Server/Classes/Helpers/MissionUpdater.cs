using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.IO;
using LibGit2Sharp;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EventSpawnClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.GlobalsClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesChangesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;

namespace DayZServerManager.Server.Classes.Helpers
{
    public class MissionUpdater
    {
        ManagerConfig config;
        private char folderSeperator;
        public MissionUpdater(ManagerConfig config) 
        {
            this.config = config;
            if (OperatingSystem.IsWindows())
            {
                folderSeperator = '\\';
            }
            else
            {
                folderSeperator = '/';
            }
        }

        public void Update()
        {
            try
            {
                //Creating path variables for later use
                string missionPath = Path.Combine(Manager.SERVER_PATH, "mpmissions", config.missionName);
                string missionTemplatePath = Path.Combine(Manager.MISSION_PATH, config.missionTemplateName);

                #region Creating directories
                List<string> serverDirectories = FileSystem.GetDirectories(Manager.SERVER_PATH).ToList<string>();
                if (serverDirectories.Find(dir => Path.GetFileName(dir) == "mpmissions") == null)
                {
                    FileSystem.CreateDirectory(Path.Combine(Manager.SERVER_PATH, "mpmissions"));
                }

                List<string> mpmissionDirectories = FileSystem.GetDirectories(Manager.MISSION_PATH).ToList<string>();
                if (mpmissionDirectories.Find(dir => Path.GetFileName(dir) == config.missionTemplateName) == null)
                {
                    FileSystem.CreateDirectory(missionTemplatePath);
                }
                #endregion Creating directories

                #region Creating example CustomFiles

                //Creating CustomFiles folder
                List<string> missionTemplateDirectories = FileSystem.GetDirectories(missionTemplatePath).ToList<string>();
                if (missionTemplateDirectories.Find(dir => Path.GetFileName(dir) == "CustomFiles") == null)
                {
                    FileSystem.CreateDirectory(Path.Combine(missionTemplatePath, "CustomFiles"));
                }

                // Creating example folder in CustomFiles
                List<string> customFilesDirectories = FileSystem.GetDirectories(Path.Combine(missionTemplatePath, "CustomFiles")).ToList<string>();
                if (customFilesDirectories.Count == 0)
                {
                    FileSystem.CreateDirectory(Path.Combine(missionTemplatePath, "CustomFiles", "ExampleModFiles"));
                    customFilesDirectories = FileSystem.GetDirectories(Path.Combine(missionTemplatePath, "CustomFiles")).ToList<string>();
                }

                //Creating Example typesFile
                List<string> filesNames = FileSystem.GetFiles(Path.Combine(customFilesDirectories[0])).ToList<string>();
                if (filesNames.Count == 0)
                {
                    TypesFile exampleTypesFile = new TypesFile()
                    {
                        typesItem = new List<TypesItem>()
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
                    XMLSerializer.SerializeXMLFile<TypesFile>(Path.Combine(customFilesDirectories[0], "exampleTypesFile.xml"), exampleTypesFile);
                }

                List<string> missionTemplateFiles = FileSystem.GetFiles(Path.Combine(missionTemplatePath)).ToList<string>();
                // Creating Exmple cfgeconomycore
                if (missionTemplateFiles.Find(x => Path.GetFileName(x) == "cfgeconomycore.xml") == null)
                {
                    EconomyCoreFile exampleEconomyCore = new EconomyCoreFile()
                    {
                        ceItems = new List<CeItem>()
                        {
                            new CeItem()
                            {
                                folder = Path.Combine("CustomFiles", Path.GetFileName(customFilesDirectories[0])),
                                fileItems = new List<FileItem>()
                                {
                                    new FileItem()
                                    {
                                        name = "exampleTypesFile.xml",
                                        type = "types"
                                    } 
                                } 
                            }
                        }
                    };
                    XMLSerializer.SerializeXMLFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"), exampleEconomyCore);
                }
                else
                {
                    EconomyCoreFile? economyCoreFile = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"));
                    if (economyCoreFile != null && economyCoreFile.ceItems == null)
                    {
                        economyCoreFile.ceItems = new List<CeItem>()
                        {
                            new CeItem()
                            {
                                folder = Path.Combine("CustomFiles", customFilesDirectories[0].Remove(customFilesDirectories[0].LastIndexOf(folderSeperator) + 1)),
                                fileItems = new List<FileItem>()
                                {
                                    new FileItem()
                                    {
                                        name = "exampleTypesFile.xml",
                                        type = "types"
                                    }
                                }
                            }
                        };
                        XMLSerializer.SerializeXMLFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"), economyCoreFile);
                    }
                }
                #endregion Create example CustomFiles

                #region Creating example rarities and types changes files
                //Creating customFilesRarities.json file
                if (missionTemplateFiles.Find(x => Path.GetFileName(x) == "customFilesRarities.json") == null)
                {
                    RarityFile customFilesRarities = new RarityFile();
                    customFilesRarities.ItemRarity = new Dictionary<string, int>();
                    customFilesRarities.ItemRarity.Add("example1", 3);
                    customFilesRarities.ItemRarity.Add("example2", 5);
                    JSONSerializer.SerializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, "customFilesRarities.json"), customFilesRarities);
                }

                //Creating vanillaRarities.json
                if (missionTemplateFiles.Find(x => Path.GetFileName(x) == "vanillaRarities.json") == null)
                {
                    RarityFile vanillaRarities = new RarityFile();
                    vanillaRarities.ItemRarity = new Dictionary<string, int>();
                    vanillaRarities.ItemRarity.Add("example1", 3);
                    vanillaRarities.ItemRarity.Add("example2", 5);
                    JSONSerializer.SerializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, "vanillaRarities.json"), vanillaRarities);
                }

                //Creating vanillaTypesChanges.json
                if (missionTemplateFiles.Find(x => Path.GetFileName(x) == "vanillaTypesChanges.json") == null)
                {
                    TypesChangesFile vanillaTypesChanges = new TypesChangesFile();
                    vanillaTypesChanges.types = new List<TypesChangesItem>
                    {
                        new TypesChangesItem()
                        {
                            name = "example1",
                            lifetime = 3888000,
                            rarity = 3
                        },
                        new TypesChangesItem()
                        {
                            name = "example2",
                            lifetime = 3888000,
                            rarity = 4
                        }
                    };

                    JSONSerializer.SerializeJSONFile<TypesChangesFile>(Path.Combine(missionTemplatePath, "vanillaTypesChanges.json"), vanillaTypesChanges);
                }

                //Creating expansionRarities.json and expansionTypesChanges.json, if Expansion is part of the mods
                if (config.clientMods != null && config.clientMods.FindAll(p => p.name.ToLower().Contains("expansion")).Count > 0)
                {
                    //Creating expansionRarities.json
                    if (missionTemplateFiles.Find(x => Path.GetFileName(x) == "expansionRarities.json") == null)
                    {
                        RarityFile expansionRarityFile = new RarityFile();
                        expansionRarityFile.ItemRarity = new Dictionary<string, int>();
                        expansionRarityFile.ItemRarity.Add("example1", 3);
                        expansionRarityFile.ItemRarity.Add("example2", 5);
                        JSONSerializer.SerializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, "expansionRarities.json"), expansionRarityFile);
                    }

                    //Creating expansionTypesChanges.json
                    if (missionTemplateFiles.Find(x => Path.GetFileName(x) == "expansionTypesChanges.json") == null)
                    {
                        TypesChangesFile expansionTypesChanges = new TypesChangesFile();
                        expansionTypesChanges.types = new List<TypesChangesItem>
                        {
                            new TypesChangesItem()
                            {
                                name = "example1",
                                lifetime = 3888000,
                                rarity = 3
                            },
                            new TypesChangesItem()
                            {
                                name = "example2",
                                lifetime = 3888000,
                                rarity = 4
                            }
                        };

                        JSONSerializer.SerializeJSONFile<TypesChangesFile>(Path.Combine(missionTemplatePath, "expansionTypesChanges.json"), expansionTypesChanges);
                    }

                    // Creating expansion folder in the missionTemplate folder, if it doesn't exist
                    if (missionTemplateDirectories.Find(x => Path.GetFileName(x) == "expansion") == null)
                    {
                        FileSystem.CreateDirectory(Path.Combine(missionTemplatePath, "expansion"));
                    }

                    // Creating settings folder in the expansion folder of the missionTemplate folder, if it doesn't exist
                    List<string> ExpansionDirectories = FileSystem.GetDirectories(Path.Combine(missionTemplatePath, "expansion")).ToList<string>();
                    if (ExpansionDirectories.Find(x => Path.GetFileName(x) == "settings") == null)
                    {
                        FileSystem.CreateDirectory(Path.Combine(missionTemplatePath, "expansion", "settings"));
                    }

                    // Creating HardlineSettings.json in the settings folder of the expansion folder of the missionTemplate folder, if it doesn't exist
                    List<string> ExpansionSettingsFiles = FileSystem.GetFiles(Path.Combine(missionTemplatePath, "expansion", "settings")).ToList<string>();
                    if (ExpansionDirectories.Find(x => Path.GetFileName(x) == "HardlineSettings.json") == null)
                    {
                        RarityFile? exampleHardlineRarity = new RarityFile
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
                            UseItemRarityForMarketPurchase = 0,
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

                        JSONSerializer.SerializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, "expansion", "settings", "HardlineSettings.json"), exampleHardlineRarity);
                    }
                }
                #endregion Creating example rarities and types changes files

                // Rename the old mission folder and copy the contents of the vanilla folder
                CopyVanillaMissionFolder(missionPath, Path.Combine(Manager.SERVER_PATH, "mpmissions", config.vanillaMissionName), config.backupPath);

                string expansionTemplatePath = Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", config.mapName);
                if (config.clientMods != null && config.clientMods.FindAll(p => p.name.ToLower().Contains("expansion")).Count > 0)
                {
                    // Get the new expansion mission template from git
                    expansionTemplatePath = DownloadExpansionTemplates(config);

                    // Copy the folder expansion_ce from the expansionTemplate to the new mission folder
                    CopyExpansionTemplateFiles(expansionTemplatePath, missionPath);
                }

                // Copy the folders CustomFiles and expansion and also the files mapgrouppos.xml, cfgweather.xml and cfgplayerspawnpoints.xml from the missionTemplate to the new mission folder
                CopyMissionTemplateFiles(missionTemplatePath, missionPath);

                if (mpmissionDirectories.Find(x => Path.GetFileName(x) == config.missionName) != null)
                {
                    // Change the variables in the globals.xml of TimeLogin to 5 and ZombieMaxCount to 500
                    GlobalsFile? globals = XMLSerializer.DeserializeXMLFile<GlobalsFile>(Path.Combine(missionPath, "db", "globals.xml"));
                    if (globals != null)
                    {
                        UpdateGlobals(globals);
                        XMLSerializer.SerializeXMLFile<GlobalsFile>(Path.Combine(missionPath, "db", "globals.xml"), globals);
                    }

                    // Add the other parts of the cfgeconomycore.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EconomyCoreFile? missionEconomyCore = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(missionPath, "cfgeconomycore.xml"));

                    if (missionEconomyCore != null)
                    {
                        EconomyCoreFile? expansionTemplateEconomyCore = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(expansionTemplatePath, "cfgeconomycore.xml"));
                        EconomyCoreFile? missionTemplateEconomyCore = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"));
                        if (expansionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, expansionTemplateEconomyCore);
                        }
                        if (missionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, missionTemplateEconomyCore);
                        }
                        XMLSerializer.SerializeXMLFile<EconomyCoreFile>(Path.Combine(missionPath, "cfgeconomycore.xml"), missionEconomyCore);
                    }

                    // Add the other parts of the cfgeventspawns.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EventSpawnsFile? missionEventSpawns = XMLSerializer.DeserializeXMLFile<EventSpawnsFile>(Path.Combine(missionPath, "cfgeventspawns.xml"));

                    if (missionEventSpawns != null)
                    {
                        EventSpawnsFile? expansionTemplateEventSpawns = XMLSerializer.DeserializeXMLFile<EventSpawnsFile>(Path.Combine(expansionTemplatePath, "cfgeventspawns.xml"));
                        EventSpawnsFile? missionTemplateEventSpawns = XMLSerializer.DeserializeXMLFile<EventSpawnsFile>(Path.Combine(missionTemplatePath, "cfgeventspawns.xml"));
                        if (expansionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, expansionTemplateEventSpawns);
                        }
                        if (missionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, missionTemplateEventSpawns);
                        }
                        XMLSerializer.SerializeXMLFile<EventSpawnsFile>(Path.Combine(missionPath, "cfgeventspawns.xml"), missionEventSpawns);
                    }

                    List<string> missionFiles = FileSystem.GetFiles(missionPath).ToList<string>();
                    // Add the part of the main method of the init.c of the missionTemplate to the one from the new mission folder
                    if (missionFiles.Find(x => Path.GetFileName(x) == "init.c") != null && missionTemplateFiles.Find(x => Path.GetFileName(x) == "init.c") != null)
                    {
                        string missionInit = InitFileSerializer.DeserializeInitFile(Path.Combine(missionPath, "init.c"));
                        string templateInit = InitFileSerializer.DeserializeInitFile(Path.Combine(missionTemplatePath, "init.c"));

                        missionInit = UpdateInit(missionInit, templateInit);

                        InitFileSerializer.SerializeInitFile(Path.Combine(missionPath, "init.c"), missionInit);
                    }

                    // Changing the types files to reflect the rarities
                    RarityFile? hardlineRarity = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json"));
                    RarityFile? vanillaRarity = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, "vanillaRarities.json"));
                    RarityFile? expansionRarity = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, "expansionRarities.json"));

                    TypesFile? vanillaTypes = XMLSerializer.DeserializeXMLFile<TypesFile>(Path.Combine(missionPath, "db", "types.xml"));
                    TypesFile? expansionTypes = XMLSerializer.DeserializeXMLFile<TypesFile>(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml"));

                    if (hardlineRarity != null)
                    {
                        RarityFile? customFilesRarityFile = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, "customFilesRarities.json"));
                        if (vanillaRarity != null)
                        {
                            UpdateHardlineRarity(hardlineRarity, vanillaRarity);
                        }
                        if (expansionRarity != null)
                        {
                            UpdateHardlineRarity(hardlineRarity, expansionRarity);
                        }
                        if (customFilesRarityFile != null)
                        {
                            UpdateHardlineRarity(hardlineRarity, customFilesRarityFile);
                        }
                        JSONSerializer.SerializeJSONFile<RarityFile>(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json"), hardlineRarity);
                    }

                    if (vanillaTypes != null)
                    {
                        if (vanillaRarity != null)
                        {
                            UpdateTypesWithRarity(vanillaTypes, vanillaRarity);
                        }

                        // Change the Lifetimes of items in the types.xml
                        TypesChangesFile? changes = JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(missionTemplatePath, "vanillaTypesChanges.json"));
                        if (changes != null)
                        {
                            UpdateLifetime(vanillaTypes, changes);
                        }

                        XMLSerializer.SerializeXMLFile<TypesFile>(Path.Combine(missionPath, "db", "types.xml"), vanillaTypes);
                    }

                    if (expansionTypes != null)
                    {
                        if (expansionRarity != null)
                        {
                            UpdateTypesWithRarity(expansionTypes, expansionRarity);
                        }

                        // Change the Lifetimes of items in the expansionTypes.xml
                        TypesChangesFile? changes = JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(missionTemplatePath, "expansionTypesChanges.json"));
                        if (changes != null)
                        {
                            UpdateLifetime(expansionTypes, changes);
                        }

                        XMLSerializer.SerializeXMLFile<TypesFile>(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml"), expansionTypes);
                    }
                }

                mpmissionDirectories = FileSystem.GetDirectories(Manager.MISSION_PATH).ToList<string>();
                if (mpmissionDirectories.Find(x => Path.GetFileName(x) == config.missionName + "Old") != null)
                {

                    List<string> oldMissionDirectories = FileSystem.GetDirectories(missionPath + "Old").ToList<string>();
                    // Copy over the data and map from the old mission into the new one
                    if (oldMissionDirectories.Find(x => Path.GetFileName(x) == "storage_1") != null)
                    {
                        CopyPersistenceData(missionPath);
                    }

                    // Move old mission to backup
                    MoveOldMission(Path.Combine(missionPath + "Old"), config.backupPath);

                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        #region Searches
        // Searches for the matching CeItem and returns it
        public CeItem? SearchForCeItem(CeItem ceItem, EconomyCoreFile cfg)
        {
            try
            {
                Manager.WriteToConsole("Searching for ce item");
                foreach (CeItem item in cfg.ceItems)
                {
                    if (item.folder.ToLower().Trim() == ceItem.folder.ToLower().Trim())
                    {
                        Manager.WriteToConsole("Finished searching for ce item");
                        return item;
                    }
                }
                Manager.WriteToConsole("Finished searching for ce item");
                return null;
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return null;
            }
        }

        // Searches for the matching FileItem and returns true, if it finds smth
        public bool SearchForFileItem(FileItem fileItem, CeItem ceItem)
        {
            try
            {
                Manager.WriteToConsole("Searching for file item");
                foreach (FileItem item in ceItem.fileItems)
                {
                    if (item.name.ToLower().Trim() == fileItem.name.ToLower().Trim())
                    {
                        Manager.WriteToConsole("Finished searching for file item");
                        return true;
                    }
                }
                Manager.WriteToConsole("Finished searching for file item");
                return false;
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return false;
            }
        }

        // Searches for the matching EventItem and returns it
        public EventItem? SearchForEventItem(EventItem eventItem, EventSpawnsFile cfg)
        {
            try
            {
                Manager.WriteToConsole("Searching for event item");
                foreach (EventItem item in cfg.eventItems)
                {
                    if (item.name.ToLower().Trim() == eventItem.name.ToLower().Trim())
                    {
                        Manager.WriteToConsole("Finished searching for event item");
                        return item;
                    }
                }
                Manager.WriteToConsole("Finished searching for event item");
                return null;
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return null;
            }
        }

        // Searches for the matching PosItem and returns true, if it finds it
        public bool SearchForPosItem(PosItem posItem, EventItem eventItem)
        {
            try
            {
                Manager.WriteToConsole("Searching for pos item");
                foreach (PosItem item in eventItem.positions)
                {
                    if (Int64.Parse(item.x) == Int64.Parse(posItem.x) && Int64.Parse(item.y) == Int64.Parse(posItem.y) && Int64.Parse(item.a) == Int64.Parse(posItem.a))
                    {
                        Manager.WriteToConsole("Finished searching for pos item");
                        return true;
                    }
                }
                Manager.WriteToConsole("Finished searching for pos item");
                return false;
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return false;
            }
        }

        // Searches for the matching TypesItem and returns it
        public TypesItem? SearchForTypesItem(string name, TypesFile typesFile)
        {
            try
            {
                Manager.WriteToConsole("Searching for types file");
                foreach (TypesItem item in typesFile.typesItem)
                {
                    if (item.name.ToLower().Trim() == name.ToLower().Trim())
                    {
                        Manager.WriteToConsole("Finished searching for types file");
                        return item;
                    }
                }
                Manager.WriteToConsole("Finished searching for types file");
                return null;
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return null;
            }
        }
        #endregion Searches

        #region UpdateFunctions
        public string UpdateInit(string init, string templateInit)
        {
            try
            {
                Manager.WriteToConsole("Updating init");
                int initStartIndex = init.IndexOf("{") + 1;
                int templateStartIndex = templateInit.IndexOf("{") + 1;
                int templateEndIndex = templateInit.LastIndexOf("}") - 1;
                int templateLength = templateEndIndex - templateStartIndex;
                if (templateLength > 0)
                {
                    string insertionString = templateInit.Substring(templateStartIndex, templateLength);
                    Manager.WriteToConsole("Finished updating init");
                    return init.Insert(initStartIndex, insertionString);
                }
                else
                {
                    Manager.WriteToConsole("Finished updating init");
                    return init;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.Message);
                return init;
            }
        }

        public void UpdateGlobals(GlobalsFile globals)
        {
            try
            {
                Manager.WriteToConsole("Updating globals");
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
                Manager.WriteToConsole("Finished updating globals"); 
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        // Updates the Rarity in the given RarityFile with the rarities of another RarityFile
        public void UpdateHardlineRarity(RarityFile hardlineRarity, RarityFile newRarities)
        {
            try
            {
                Manager.WriteToConsole("Updating types with rarity");
                foreach (string key in newRarities.ItemRarity.Keys)
                {
                    if (hardlineRarity.ItemRarity.ContainsKey(key.ToLower()))
                    {
                        hardlineRarity.ItemRarity[key.ToLower()] = newRarities.ItemRarity[key];
                    }
                    else if (hardlineRarity.ItemRarity.ContainsKey(key))
                    {
                        hardlineRarity.ItemRarity[key] = newRarities.ItemRarity[key];
                    }
                    else
                    {
                        hardlineRarity.ItemRarity.Add(key, newRarities.ItemRarity[key]);
                    }
                }
                Manager.WriteToConsole("Finished updating types with rarity");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        // Updates the spawning of items in the given TypesFile with the new spawns of another TypesFile
        public void UpdateTypesWithRarity(TypesFile typesFile, RarityFile rarityFile)
        {
            try
            {
                Manager.WriteToConsole("Updating types with rarity");
                foreach (string key in rarityFile.ItemRarity.Keys)
                {
                    TypesItem? item = SearchForTypesItem(key, typesFile);
                    if (item != null)
                    {
                        switch (rarityFile.ItemRarity[key])
                        {
                            case 0:
                                item.nominal = 0;
                                item.min = 0;
                                break;
                            case 1:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 160;
                                    item.min = 80;
                                }
                                else
                                {
                                    item.nominal = 320;
                                    item.min = 160;
                                }
                                break;
                            case 2:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 80;
                                    item.min = 40;
                                }
                                else
                                {
                                    item.nominal = 160;
                                    item.min = 80;
                                }
                                break;
                            case 3:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 40;
                                    item.min = 20;
                                }
                                else
                                {
                                    item.nominal = 80;
                                    item.min = 40;
                                }
                                break;
                            case 4:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 20;
                                    item.min = 10;
                                }
                                else
                                {
                                    item.nominal = 40;
                                    item.min = 20;
                                }
                                break;
                            case 5:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 10;
                                    item.min = 5;
                                }
                                else
                                {
                                    item.nominal = 20;
                                    item.min = 10;
                                }
                                break;
                            case 6:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 5;
                                    item.min = 2;
                                }
                                else
                                {
                                    item.nominal = 10;
                                    item.min = 5;
                                }
                                break;
                            case 7:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 2;
                                    item.min = 1;
                                }
                                else
                                {
                                    item.nominal = 5;
                                    item.min = 2;
                                }
                                break;
                            case 8:
                                if (item.category != null && item.category.name == "clothes")
                                {
                                    item.nominal = 1;
                                    item.min = 1;
                                }
                                else
                                {
                                    item.nominal = 2;
                                    item.min = 1;
                                }
                                break;
                        }
                    }
                }
                Manager.WriteToConsole("Finished updating types with rarity");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
        
        // Updates the lifetime of items in the given TypesFile with the new spawns of another TypesFile
        public void UpdateLifetime(TypesFile typesFile, TypesChangesFile changesFile)
        {
            try
            {
                Manager.WriteToConsole("Updating lifetimes");
                foreach (TypesChangesItem change in changesFile.types)
                {
                    TypesItem? item = SearchForTypesItem(change.name, typesFile);
                    if (item != null)
                    {
                        item.lifetime = change.lifetime;
                    }
                }
                Manager.WriteToConsole("Finished updating lifetimes");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public void UpdateEventSpawns(EventSpawnsFile missionEventSpawns, EventSpawnsFile templateEventSpawns)
        {
            try 
            {
                Manager.WriteToConsole("Updating event spawns");
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
                Manager.WriteToConsole("Finished updating event spawns");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString()); 
            }
        }

        public void UpdateEconomyCore(EconomyCoreFile economyCoreFile, EconomyCoreFile templateEconomyCoreFile)
        {
            try
            {
                Manager.WriteToConsole("Updating economy core");
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
                Manager.WriteToConsole("Finsihed updating Economy Core");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
        #endregion UpdateFunctions

        #region CopyFunctions
        public void CopyVanillaMissionFolder(string missionPath, string vanillaMissionPath, string backupPath)
        {
            try
            {
                Manager.WriteToConsole("Copying vanilla mission files and folders");
                if (FileSystem.DirectoryExists(vanillaMissionPath))
                {
                    if (FileSystem.DirectoryExists(missionPath))
                    {
                        if (FileSystem.DirectoryExists(missionPath + "Old"))
                        {
                            MoveOldMission(missionPath, backupPath);
                        }
                        FileSystem.MoveDirectory(missionPath, missionPath + "Old");
                    }
                    FileSystem.CopyDirectory(vanillaMissionPath, missionPath);
                }
                Manager.WriteToConsole("Finished copying vanilla mission files and folders");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
        
        public void CopyExpansionTemplateFiles(string expansionTemplatePath, string missionPath)
        {
            try
            {
                Manager.WriteToConsole("Copying expansion template files");
                if (FileSystem.DirectoryExists(expansionTemplatePath))
                {
                    FileSystem.CopyDirectory(Path.Combine(expansionTemplatePath, "expansion_ce"), Path.Combine(missionPath, "expansion_ce"), true);
                }
                Manager.WriteToConsole("Finished copying expansion template files");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public void CopyMissionTemplateFiles(string missionTemplatePath, string missionPath)
        {
            try
            {
                Manager.WriteToConsole("Moving mission template files and folders");
                if (FileSystem.DirectoryExists(missionTemplatePath))
                {
                    if (FileSystem.DirectoryExists(Path.Combine(missionTemplatePath, "CustomFiles")))
                    {
                        FileSystem.CopyDirectory(Path.Combine(missionTemplatePath, "CustomFiles"), Path.Combine(missionPath, "CustomFiles"), true);
                    }
                    if (FileSystem.DirectoryExists(Path.Combine(missionTemplatePath, "expansion")))
                    {
                        FileSystem.CopyDirectory(Path.Combine(missionTemplatePath, "expansion"), Path.Combine(missionPath, "expansion"), true);
                    }
                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "mapgrouppos.xml")))
                    {
                        FileSystem.CopyFile(Path.Combine(missionTemplatePath, "mapgrouppos.xml"), Path.Combine(missionPath, "mapgrouppos.xml"), true);
                    }
                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "cfgweather.xml")))
                    {
                        FileSystem.CopyFile(Path.Combine(missionTemplatePath, "cfgweather.xml"), Path.Combine(missionPath, "cfgweather.xml"), true);
                    }
                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "cfgplayerspawnpoints.xml")))
                    {
                        FileSystem.CopyFile(Path.Combine(missionTemplatePath, "cfgplayerspawnpoints.xml"), Path.Combine(missionPath, "cfgplayerspawnpoints.xml"), true);
                    }
                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "cfggameplay.json")))
                    {
                        FileSystem.CopyFile(Path.Combine(missionTemplatePath, "cfggameplay.json"), Path.Combine(missionPath, "cfggameplay.json"), true);
                    }
                }
                Manager.WriteToConsole("Finshed moving mission template files and folders");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
        
        public void CopyPersistenceData(string missionPath)
        {
            try
            {
                Manager.WriteToConsole("Copying old persistance data");
                FileSystem.CopyDirectory(Path.Combine(missionPath + "Old", "storage_1"), Path.Combine(missionPath, "storage_1"));
                Manager.WriteToConsole("Finished copy old persistance data");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public void MoveOldMission(string oldPath, string backupPath)
        {
            try
            {
                Manager.WriteToConsole("Moving old mission");
                string newPath = Path.Combine(backupPath, "FullMissionBackups", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                if (!FileSystem.DirectoryExists(Path.Combine(backupPath, "FullMissionBackups")))
                {
                    FileSystem.CreateDirectory(Path.Combine(backupPath, "FullMissionBackups"));
                }
                FileSystem.MoveDirectory(oldPath, newPath);
                Manager.WriteToConsole("Finished moving old mission");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
        #endregion CopyFunctions

        #region DownloadFunctions
        public string DownloadExpansionTemplates(ManagerConfig config)
        {
            try
            {
                Manager.WriteToConsole("Downloading expansion template");
                if (FileSystem.DirectoryExists(Manager.EXPANSION_DOWNLOAD_PATH))
                {
                    Repository rep = new Repository(Manager.EXPANSION_DOWNLOAD_PATH);
                    PullOptions pullOptions = new PullOptions();
                    pullOptions.FetchOptions = new FetchOptions();
                    Commands.Pull(rep, new Signature("username", "email", new DateTimeOffset(DateTime.Now)), pullOptions);

                    Manager.WriteToConsole("Finished downloading expansion template");

                    if (FileSystem.DirectoryExists(Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", config.mapName)))
                    {
                        return Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", config.mapName);
                    }
                    else if (FileSystem.DirectoryExists(Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", "0_INCOMPLETE", config.mapName)))
                    {
                        return Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", "0_INCOMPLETE", config.mapName);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    if (Manager.EXPANSION_DOWNLOAD_PATH == string.Empty)
                    {
                        Manager.EXPANSION_DOWNLOAD_PATH = Path.Combine(Manager.SERVER_PATH, "mpmissions", "DayZ-Expansion-Missions");
                    }

                    Repository.Clone("https://github.com/ExpansionModTeam/DayZ-Expansion-Missions.git", Manager.EXPANSION_DOWNLOAD_PATH);

                    Manager.WriteToConsole("Finished downloading expansion template");

                    if (FileSystem.DirectoryExists(Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", config.mapName)))
                    {
                        return Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", config.mapName);
                    }
                    else if (FileSystem.DirectoryExists(Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", "0_INCOMPLETE", config.mapName)))
                    {
                        return Path.Combine(Manager.EXPANSION_DOWNLOAD_PATH, "Template", "0_INCOMPLETE", config.mapName);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return string.Empty;
            }
        }
        #endregion DownloadFunctions
    }
}
