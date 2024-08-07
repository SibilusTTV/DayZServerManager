﻿using Microsoft.VisualBasic.FileIO;
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
        public MissionUpdater(ManagerConfig config) 
        {
            this.config = config;
        }

        public void Update()
        {
            try
            {
                //Creating path variables for later use
                string missionPath = Path.Combine(config.serverPath, "mpmissions", config.missionName);
                string missionTemplatePath = Path.Combine(config.missionTemplatePath);

                #region Creating directories
                if (!FileSystem.DirectoryExists(Path.Combine(config.serverPath, "mpmissions")))
                {
                    FileSystem.CreateDirectory(Path.Combine(config.serverPath, "mpmissions"));
                }

                if (!FileSystem.DirectoryExists(missionTemplatePath))
                {
                    FileSystem.CreateDirectory(missionTemplatePath);
                }

                if (!FileSystem.DirectoryExists(Path.Combine(missionTemplatePath, "CustomFiles")))
                {
                    FileSystem.CreateDirectory(Path.Combine(missionTemplatePath, "CustomFiles"));
                }
                #endregion Creating directories

                #region Creating example CustomFiles
                //Creating CustomFiles folder
                List<string> directoryNames = FileSystem.GetDirectories(Path.Combine(missionTemplatePath, "CustomFiles")).ToList<string>();
                if (directoryNames.Count == 0)
                {
                    FileSystem.CreateDirectory(Path.Combine(missionTemplatePath, "CustomFiles", "ExampleModFiles"));
                    directoryNames = FileSystem.GetDirectories(Path.Combine(missionTemplatePath, "CustomFiles")).ToList<string>();
                }

                //Creating Example typesFile
                List<string> filesNames = FileSystem.GetFiles(Path.Combine(directoryNames[0])).ToList<string>();
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
                    TypesFileSerializer.SerializeTypesFile(Path.Combine(directoryNames[0], "exampleTypesFile.xml"), exampleTypesFile);
                }
                
                // Creating Exmple cfgeconomycore
                if (!FileSystem.FileExists(Path.Combine(missionTemplatePath, "cfgeconomycore.xml")))
                {
                    EconomyCoreFile exampleEconomyCore = new EconomyCoreFile()
                    {
                        ceItems = new List<CeItem>()
                        {
                            new CeItem()
                            {
                                folder = Path.Combine("CustomFiles", directoryNames[0].Remove(directoryNames[0].LastIndexOf("\\") + 1)),
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
                    EconomyCoreFileSerializer.SerializeEconomyCoreFile(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"), exampleEconomyCore);
                }
                else
                {
                    EconomyCoreFile? economyCoreFile = EconomyCoreFileSerializer.DeserializeEconomyCoreFile(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"));
                    if (economyCoreFile != null)
                    {
                        if (economyCoreFile.ceItems == null)
                        {
                            economyCoreFile.ceItems = new List<CeItem>()
                            {
                                new CeItem()
                                {
                                    folder = Path.Combine("CustomFiles", directoryNames[0].Remove(directoryNames[0].LastIndexOf("\\") + 1)),
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
                            EconomyCoreFileSerializer.SerializeEconomyCoreFile(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"), economyCoreFile);
                        }
                    }
                }
                #endregion Create example CustomFiles

                #region Creating example rarities files
                //Creating VanillaRarities file
                if (!FileSystem.FileExists(Path.Combine(missionTemplatePath, "vanillaRarities.json")))
                {
                    RarityFile vanillaRarities = new RarityFile();
                    vanillaRarities.ItemRarity = new Dictionary<string, int>();
                    vanillaRarities.ItemRarity.Add("example1", 3);
                    vanillaRarities.ItemRarity.Add("example2", 5);
                    RarityFileSerializer.SerializeRarityFile(Path.Combine(missionTemplatePath, "vanillaRarities.json"), vanillaRarities);
                }

                //Creating ExpansionRarities, if Expansion is part of the mods
                if (config.clientMods != null && config.clientMods.Select(p => p.workshopID == 2116157322 || p.workshopID == 2572331007) != null)
                {
                    if (!FileSystem.FileExists(Path.Combine(missionTemplatePath, "expansionTypesChanges.json")))
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

                        TypesChangesFileSerializer.SerializeTypesChangesFile(Path.Combine(missionTemplatePath, "expansionTypesChanges.json"), expansionTypesChanges);
                    }
                }
                #endregion Creating example rarities files

                #region Creating example TypesChanges file
                if (!FileSystem.FileExists(Path.Combine(missionTemplatePath, "vanillaTypesChanges.json")))
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

                    TypesChangesFileSerializer.SerializeTypesChangesFile(Path.Combine(missionTemplatePath, "vanillaTypesChanges.json"), vanillaTypesChanges);
                }
                #endregion Creating example TypesChanges file

                // Rename the old mission folder and copy the contents of the vanilla folder
                CopyMissionFolder(missionPath, Path.Combine(config.serverPath, "mpmissions", config.vanillaMissionName), config.backupPath);

                string expansionTemplatePath = Path.Combine(config.expansionDownloadPath, "Template", config.mapName);
                if (config.clientMods != null && config.clientMods.Select(p => p.workshopID == 2116157322 || p.workshopID == 2572331007) != null)
                {
                    // Get the new expansion mission template from git
                    expansionTemplatePath = DownloadExpansionTemplates(config);

                    // Copy the folder expansion_ce from the expansionTemplate to the new mission folder
                    CopyExpansionTemplateFiles(expansionTemplatePath, missionPath);
                }

                // Copy the folders CustomFiles and expansion and also the files mapgrouppos.xml, cfgweather.xml and cfgplayerspawnpoints.xml from the missionTemplate to the new mission folder
                CopyMissionTemplateFiles(missionTemplatePath, missionPath);

                if (FileSystem.DirectoryExists(missionPath))
                {
                    // Change the variables in the globals.xml of TimeLogin to 5 and ZombieMaxCount to 500
                    GlobalsFile? globals = GlobalsFileSerializer.DeserializeGlobalsFile(Path.Combine(missionPath, "db", "globals.xml"));
                    if (globals != null)
                    {
                        UpdateGlobals(globals);
                        GlobalsFileSerializer.SerializeGlobalsFile(Path.Combine(missionPath, "db", "globals.xml"), globals);
                    }

                    // Add the other parts of the cfgeconomycore.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EconomyCoreFile? missionEconomyCore = EconomyCoreFileSerializer.DeserializeEconomyCoreFile(Path.Combine(missionPath, "cfgeconomycore.xml"));

                    if (missionEconomyCore != null)
                    {
                        EconomyCoreFile? expansionTemplateEconomyCore = EconomyCoreFileSerializer.DeserializeEconomyCoreFile(Path.Combine(expansionTemplatePath, "cfgeconomycore.xml"));
                        EconomyCoreFile? missionTemplateEconomyCore = EconomyCoreFileSerializer.DeserializeEconomyCoreFile(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"));
                        if (expansionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, expansionTemplateEconomyCore);
                        }
                        if (missionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, missionTemplateEconomyCore);
                        }
                        EconomyCoreFileSerializer.SerializeEconomyCoreFile(Path.Combine(missionPath, "cfgeconomycore.xml"), missionEconomyCore);
                    }

                    // Add the other parts of the cfgeventspawns.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EventSpawnsFile? missionEventSpawns = EventSpawnsSerializer.DesererializeEventSpawns(Path.Combine(missionPath, "cfgeventspawns.xml"));

                    if (missionEventSpawns != null)
                    {
                        EventSpawnsFile? expansionTemplateEventSpawns = EventSpawnsSerializer.DesererializeEventSpawns(Path.Combine(expansionTemplatePath, "cfgeventspawns.xml"));
                        EventSpawnsFile? missionTemplateEventSpawns = EventSpawnsSerializer.DesererializeEventSpawns(Path.Combine(missionTemplatePath, "cfgeventspawns.xml"));
                        if (expansionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, expansionTemplateEventSpawns);
                        }
                        if (missionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, missionTemplateEventSpawns);
                        }
                        EventSpawnsSerializer.SerializeEventSpawns(Path.Combine(missionPath, "cfgeventspawns.xml"), missionEventSpawns);
                    }

                    // Add the part of the main method of the init.c of the missionTemplate to the one from the new mission folder
                    if (FileSystem.FileExists(Path.Combine(missionPath, "init.c")) && FileSystem.FileExists(Path.Combine(missionTemplatePath, "init.c")))
                    {
                        string missionInit = InitFileSerializer.DeserializeInitFile(Path.Combine(missionPath, "init.c"));
                        string templateInit = InitFileSerializer.DeserializeInitFile(Path.Combine(missionTemplatePath, "init.c"));

                        missionInit = UpdateInit(missionInit, templateInit);

                        InitFileSerializer.SerializeInitFile(Path.Combine(missionPath, "init.c"), missionInit);
                    }

                    // Changing the types files to reflect the rarities
                    RarityFile? hardlineRarity = RarityFileSerializer.DeserializeRarityFile(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json"));
                    RarityFile? vanillaRarity = RarityFileSerializer.DeserializeRarityFile(Path.Combine(missionTemplatePath, "vanillaRarities.json"));
                    RarityFile? expansionRarity = RarityFileSerializer.DeserializeRarityFile(Path.Combine(missionTemplatePath, "expansionRarities.json"));

                    TypesFile? vanillaTypes = TypesFileSerializer.DeserializeTypesFile(Path.Combine(missionPath, "db", "types.xml"));
                    TypesFile? expansionTypes = TypesFileSerializer.DeserializeTypesFile(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml"));

                    if (hardlineRarity != null)
                    {
                        RarityFile? customFilesRarityFile = RarityFileSerializer.DeserializeRarityFile(Path.Combine(missionTemplatePath, "customFilesRarities.json"));
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
                        RarityFileSerializer.SerializeRarityFile(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json"), hardlineRarity);
                    }

                    if (vanillaTypes != null)
                    {
                        if (vanillaRarity != null)
                        {
                            UpdateTypesWithRarity(vanillaTypes, vanillaRarity);
                        }

                        // Change the Lifetimes of items in the types.xml
                        TypesChangesFile? changes = TypesChangesFileSerializer.DeserializeTypesChangesFile(Path.Combine(missionTemplatePath, "vanillaTypesChanges.json"));
                        if (changes != null)
                        {
                            UpdateLifetime(vanillaTypes, changes);
                        }

                        TypesFileSerializer.SerializeTypesFile(Path.Combine(missionPath, "db", "types.xml"), vanillaTypes);
                    }

                    if (expansionTypes != null)
                    {
                        if (expansionRarity != null)
                        {
                            UpdateTypesWithRarity(expansionTypes, expansionRarity);
                        }

                        // Change the Lifetimes of items in the expansionTypes.xml
                        TypesChangesFile? changes = TypesChangesFileSerializer.DeserializeTypesChangesFile(Path.Combine(missionTemplatePath, "expansionTypesChanges.json"));
                        if (changes != null)
                        {
                            UpdateLifetime(expansionTypes, changes);
                        }

                        TypesFileSerializer.SerializeTypesFile(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml"), expansionTypes);
                    }
                }

                // Copy over the data and map from the old mission into the new one
                if (FileSystem.DirectoryExists(Path.Combine(missionPath + "Old", "storage_1")))
                {
                    CopyPersistenceData(missionPath);
                }

                // Delete the old mission
                if (FileSystem.DirectoryExists(Path.Combine(missionPath + "Old")))
                {
                    CopyOldMission(Path.Combine(missionPath + "Old"), config.backupPath);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        #region Searches
        // Searches for the matching CeItem and returns it
        public CeItem? SearchForCeItem(CeItem ceItem, EconomyCoreFile cfg)
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
        public bool SearchForFileItem(FileItem fileItem, CeItem ceItem)
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
        public EventItem? SearchForEventItem(EventItem eventItem, EventSpawnsFile cfg)
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
        public bool SearchForPosItem(PosItem posItem, EventItem eventItem)
        {
            foreach (PosItem item in eventItem.positions)
            {
                if (Int64.Parse(item.x) == Int64.Parse(posItem.x) && Int64.Parse(item.y) == Int64.Parse(posItem.y) && Int64.Parse(item.a) == Int64.Parse(posItem.a))
                {
                    return true;
                }
            }
            return false;
        }

        // Searches for the matching TypesItem and returns it
        public TypesItem? SearchForTypesItem(string name, TypesFile typesFile)
        {
            foreach (TypesItem item in typesFile.typesItem)
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
        public string UpdateInit(string init, string templateInit)
        {
            int initStartIndex = init.IndexOf("{") + 1;
            int templateStartIndex = templateInit.IndexOf("{") + 1;
            int templateEndIndex = templateInit.LastIndexOf("}") - 1;
            int templateLength = templateEndIndex - templateStartIndex;
            if (templateLength > 0)
            {
                string insertionString = templateInit.Substring(templateStartIndex, templateLength);
                return init.Insert(initStartIndex, insertionString);
            }
            else
            {
                return init;
            }
        }

        public void UpdateGlobals(GlobalsFile globals)
        {
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
        }

        // Updates the Rarity in the given RarityFile with the rarities of another RarityFile
        public void UpdateHardlineRarity(RarityFile hardlineRarity, RarityFile newRarities)
        {
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
        }

        // Updates the spawning of items in the given TypesFile with the new spawns of another TypesFile
        public void UpdateTypesWithRarity(TypesFile typesFile, RarityFile rarityFile)
        {
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
        }
        
        // Updates the lifetime of items in the given TypesFile with the new spawns of another TypesFile
        public void UpdateLifetime(TypesFile typesFile, TypesChangesFile changesFile)
        {
            foreach (TypesChangesItem change in changesFile.types)
            {
                TypesItem? item = SearchForTypesItem(change.name, typesFile);
                if (item != null)
                {
                    item.lifetime = change.lifetime;
                }
            }
        }

        public void UpdateEventSpawns(EventSpawnsFile missionEventSpawns, EventSpawnsFile templateEventSpawns)
        {
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
        }

        public void UpdateEconomyCore(EconomyCoreFile economyCoreFile, EconomyCoreFile templateEconomyCoreFile)
        {
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
        }
        #endregion UpdateFunctions

        #region CopyFunctions
        public void CopyMissionFolder(string missionPath, string vanillaMissionPath, string backupPath)
        {
            try
            {
                if (FileSystem.DirectoryExists(vanillaMissionPath))
                {
                    if (FileSystem.DirectoryExists(missionPath))
                    {
                        if (FileSystem.DirectoryExists(missionPath + "Old"))
                        {
                            CopyOldMission(missionPath, backupPath);
                        }
                        FileSystem.MoveDirectory(missionPath, missionPath + "Old");
                    }
                    FileSystem.CopyDirectory(vanillaMissionPath, missionPath);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }
        
        public void CopyExpansionTemplateFiles(string expansionTemplatePath, string missionPath)
        {
            try
            {
                if (FileSystem.DirectoryExists(expansionTemplatePath))
                {
                    FileSystem.CopyDirectory(Path.Combine(expansionTemplatePath, "expansion_ce"), Path.Combine(missionPath, "expansion_ce"), true);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public void CopyMissionTemplateFiles(string missionTemplatePath, string missionPath)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }
        
        public void CopyPersistenceData(string missionPath)
        {
            try
            {
                FileSystem.CopyDirectory(Path.Combine(missionPath + "Old", "storage_1"), Path.Combine(missionPath, "storage_1"));
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public void CopyOldMission(string oldPath, string backupPath)
        {
            try
            {
                string newPath = Path.Combine(backupPath, "FullMissionBackups", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                if (!FileSystem.DirectoryExists(Path.Combine(backupPath, "FullMissionBackups")))
                {
                    FileSystem.CreateDirectory(Path.Combine(backupPath, "FullMissionBackups"));
                }
                FileSystem.MoveDirectory(oldPath, newPath);
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }
        #endregion CopyFunctions

        #region DownloadFunctions
        public string DownloadExpansionTemplates(ManagerConfig config)
        {
            try
            {
                if (FileSystem.DirectoryExists(config.expansionDownloadPath))
                {
                    Repository rep = new Repository(config.expansionDownloadPath);
                    PullOptions pullOptions = new PullOptions();
                    pullOptions.FetchOptions = new FetchOptions();
                    Commands.Pull(rep, new Signature("username", "email", new DateTimeOffset(DateTime.Now)), pullOptions);

                    if (FileSystem.DirectoryExists(Path.Combine(config.expansionDownloadPath, "Template", config.mapName)))
                    {
                        return Path.Combine(config.expansionDownloadPath, "Template", config.mapName);
                    }
                    else if (FileSystem.DirectoryExists(Path.Combine(config.expansionDownloadPath, "Template", "0_INCOMPLETE", config.mapName)))
                    {
                        return Path.Combine(config.expansionDownloadPath, "Template", "0_INCOMPLETE", config.mapName);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    if (config.expansionDownloadPath == string.Empty)
                    {
                        config.expansionDownloadPath = Path.Combine(config.serverPath, "mpmissions", "DayZ-Expansion-Missions");
                    }

                    Repository.Clone("https://github.com/ExpansionModTeam/DayZ-Expansion-Missions.git", config.expansionDownloadPath);

                    if (FileSystem.DirectoryExists(Path.Combine(config.expansionDownloadPath, "Template", config.mapName)))
                    {
                        return Path.Combine(config.expansionDownloadPath, "Template", config.mapName);
                    }
                    else if (FileSystem.DirectoryExists(Path.Combine(config.expansionDownloadPath, "Template", "0_INCOMPLETE", config.mapName)))
                    {
                        return Path.Combine(config.expansionDownloadPath, "Template", "0_INCOMPLETE", config.mapName);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                return string.Empty;
            }
        }
        #endregion DownloadFunctions

        private void WriteToConsole(string message)
        {
            System.Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
        }

    }
}
