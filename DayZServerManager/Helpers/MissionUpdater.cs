using DayZServerManager.ManagerConfigClasses;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using DayZServerManager.MissionClasses.TypesClasses;
using DayZServerManager.MissionClasses.RarityClasses;
using System.Xml.Serialization;
using DayZServerManager.MissionClasses.GlobalsClasses;
using DayZServerManager.MissionClasses.EconomyCoreClasses;
using System.Xml;
using DayZServerManager.MissionClasses.EventSpawnClasses;
using System.ComponentModel;
using DayZServerManager.MissionClasses.TypesChangesClasses;
using System.Reflection.Metadata;
using System.IO;

namespace DayZServerManager.Helpers
{
    public class MissionUpdater
    {
        Config config;
        public MissionUpdater(Config config) 
        {
            this.config = config;
        }

        public void Update()
        {
            try
            {
                string missionPath = Path.Combine(config.serverPath, "mpmissions", config.missionName);
                string missionTemplatePath = Path.Combine(config.missionTemplatePath);

                // Get the new expansion mission template from git
                string expansionTemplatePath = DownloadExpansionTemplates(config);

                // Rename the old mission folder and copy the contents of the vanilla folder
                CopyMissionFolder(missionPath, Path.Combine(config.serverPath, "mpmissions", config.vanillaMissionName));

                if (FileSystem.DirectoryExists(missionPath))
                {
                    // Change the variables in the globals.xml of TimeLogin to 5 and ZombieMaxCount to 500
                    GlobalsFile? globals = DeserializeGlobalsFile(Path.Combine(missionPath, "db", "globals.xml"));
                    if (globals != null)
                    {
                        UpdateGlobals(globals);
                        SerializeGlobalsFile(Path.Combine(missionPath, "db", "globals.xml"), globals);
                    }

                    // Add the other parts of the cfgeconomycore.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EconomyCoreFile? missionEconomyCore = DeserializeEconomyCoreFile(Path.Combine(missionPath, "cfgeconomycore.xml"));

                    if (missionEconomyCore != null)
                    {
                        EconomyCoreFile? expansionTemplateEconomyCore = DeserializeEconomyCoreFile(Path.Combine(expansionTemplatePath, "cfgeconomycore.xml"));
                        EconomyCoreFile? missionTemplateEconomyCore = DeserializeEconomyCoreFile(Path.Combine(missionTemplatePath, "cfgeconomycore.xml"));
                        if (expansionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, expansionTemplateEconomyCore);
                        }
                        if (missionTemplateEconomyCore != null)
                        {
                            UpdateEconomyCore(missionEconomyCore, missionTemplateEconomyCore);
                        }
                        SerializeEconomyCoreFile(Path.Combine(missionPath, "cfgeconomycore.xml"), missionEconomyCore);
                    }

                    // Add the other parts of the cfgeventspawns.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                    EventSpawnsFile? missionEventSpawns = DesererializeEventSpawns(Path.Combine(missionPath, "cfgeventspawns.xml"));

                    if (missionEventSpawns != null)
                    {
                        EventSpawnsFile? expansionTemplateEventSpawns = DesererializeEventSpawns(Path.Combine(expansionTemplatePath, "cfgeventspawns.xml"));
                        EventSpawnsFile? missionTemplateEventSpawns = DesererializeEventSpawns(Path.Combine(missionTemplatePath, "cfgeventspawns.xml"));
                        if (expansionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, expansionTemplateEventSpawns);
                        }
                        if (missionTemplateEventSpawns != null)
                        {
                            UpdateEventSpawns(missionEventSpawns, missionTemplateEventSpawns);
                        }
                        SerializeEventSpawns(Path.Combine(missionPath, "cfgeventspawns.xml"), missionEventSpawns);
                    }

                    // Add the part of the main method of the init.c of the missionTemplate to the one from the new mission folder
                    if (FileSystem.FileExists(Path.Combine(missionPath, "init.c")) && FileSystem.FileExists(Path.Combine(missionTemplatePath, "init.c")))
                    {
                        string missionInit = DeserializeInitFile(Path.Combine(missionPath, "init.c"));
                        string templateInit = DeserializeInitFile(Path.Combine(missionTemplatePath, "init.c"));

                        missionInit = UpdateInit(missionInit, templateInit);
                        
                        SerializeInitFile(Path.Combine(missionPath, "init.c"), missionInit);
                    }

                    // Copy the folder expansion_ce from the expansionTemplate to the new mission folder
                    CopyExpansionFiles(expansionTemplatePath, missionPath);

                    // Copy the folders CustomFiles and expansion and also the files mapgrouppos.xml, cfgweather.xml and cfgplayerspawnpoints.xml from the missionTemplate to the new mission folder
                    CopyMissionTemplateFiles(missionTemplatePath, missionPath);

                    // Changing the types files to reflect the rarities
                    RarityFile? hardlineRarity = DeserializeRarityFile(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json"));
                    RarityFile? vanillaRarity = DeserializeRarityFile(Path.Combine(missionTemplatePath, "vanillaRarities.json"));
                    RarityFile? expansionRarity = DeserializeRarityFile(Path.Combine(missionTemplatePath, "expansionRarities.json"));

                    TypesFile? vanillaTypes = DeserializeTypesFile(Path.Combine(missionPath, "db", "types.xml"));
                    TypesFile? expansionTypes = DeserializeTypesFile(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml"));

                    if (hardlineRarity != null)
                    {
                        if (vanillaRarity != null)
                        {
                            UpdateHardlineRarity(hardlineRarity, vanillaRarity);
                        }
                        if (expansionRarity != null)
                        {
                            UpdateHardlineRarity(hardlineRarity, expansionRarity);
                            hardlineRarity.ShowHardlineHUD = expansionRarity.ShowHardlineHUD;
                            hardlineRarity.UseHumanity = expansionRarity.UseHumanity;
                            hardlineRarity.EnableItemRarity = expansionRarity.EnableItemRarity;
                        }
                        SerializeRarityFile(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json"), hardlineRarity);
                    }

                    if (vanillaTypes != null)
                    {
                        if (vanillaRarity != null)
                        {
                            UpdateTypesWithRarity(vanillaTypes, vanillaRarity);
                        }

                        // Change the Lifetimes of items in the types.xml
                        TypesChangesFile? changes = DeserializeTypesChangesFile(Path.Combine(missionTemplatePath, "vanillaTypesChanges.json"));
                        if (changes != null)
                        {
                            UpdateLifetime(vanillaTypes, changes);
                        }

                        SerializeTypesFile(Path.Combine(missionPath, "db", "types.xml"), vanillaTypes);
                    }

                    if (expansionTypes != null)
                    {
                        if (expansionRarity != null)
                        {
                            UpdateTypesWithRarity(expansionTypes, expansionRarity);
                        }

                        // Change the Lifetimes of items in the expansionTypes.xml
                        TypesChangesFile? changes = DeserializeTypesChangesFile(Path.Combine(missionTemplatePath, "expansionTypesChanges.json"));
                        if (changes != null)
                        {
                            UpdateLifetime(expansionTypes, changes);
                        }

                        SerializeTypesFile(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml"), expansionTypes);
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
                    DeleteOldMission(missionPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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
            int startIndex = init.IndexOf("{") + 1;
            int endIndex = init.IndexOf("}") + 1;
            string insertionString = templateInit.Substring(startIndex, startIndex - endIndex);
            return init.Insert(startIndex, insertionString);
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
                            item.nominal = 320;
                            item.min = 160;
                            break;
                        case 2:
                            item.nominal = 160;
                            item.min = 80;
                            break;
                        case 3:
                            item.nominal = 80;
                            item.min = 40;
                            break;
                        case 4:
                            item.nominal = 40;
                            item.min = 20;
                            break;
                        case 5:
                            item.nominal = 20;
                            item.min = 10;
                            break;
                        case 6:
                            item.nominal = 10;
                            item.min = 5;
                            break;
                        case 7:
                            item.nominal = 5;
                            item.min = 2;
                            break;
                        case 8:
                            item.nominal = 2;
                            item.min = 1;
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

        #region SerializationFunctions
        public void SerializeInitFile(string path, string initFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(initFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }
        
        public void SerializeTypesFile(string path, TypesFile typesFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                    serializer.Serialize(writer, typesFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public void SerializeRarityFile(string path, RarityFile rarityFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(rarityFile, options);
                    writer.Write(json);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public void SerializeGlobalsFile(string path, GlobalsFile globals)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GlobalsFile));
                    serializer.Serialize(writer, globals);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public void SerializeEconomyCoreFile(string path, EconomyCoreFile economyCoreFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                    serializer.Serialize(writer, economyCoreFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public void SerializeEventSpawns(string path, EventSpawnsFile eventSpawnsFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "cfgeventspawns.xml")))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                    serializer.Serialize(writer, eventSpawnsFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }
        #endregion SerializationFunctions

        #region DeserializationFunctions
        public string DeserializeInitFile(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return string.Empty;
            }
        }

        // Takes a path and returns the deserialized TypesFile
        public TypesFile? DeserializeTypesFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                        return (TypesFile?)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return null;
            }
        }

        // Takes a path and returns the deserialized RarityFile
        public RarityFile? DeserializeRarityFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<RarityFile>(json);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return null;
            }
        }

        public TypesChangesFile? DeserializeTypesChangesFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TypesChangesFile));
                        return (TypesChangesFile?)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return null;
            }
        }

        public GlobalsFile? DeserializeGlobalsFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(GlobalsFile));
                        return (GlobalsFile?)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return null;
            }
        }

        public EconomyCoreFile? DeserializeEconomyCoreFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                        return (EconomyCoreFile?)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return null;
            }
        }

        public EventSpawnsFile? DesererializeEventSpawns(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                        return (EventSpawnsFile?)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return null;
            }
        }
        #endregion DeserializationFunctions

        #region CopyFunctions
        public void CopyMissionFolder(string missionPath, string vanillaMissionPath)
        {
            try
            {
                if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, "mpmissions", config.vanillaMissionName)))
                {
                    if (FileSystem.DirectoryExists(missionPath))
                    {
                        if (FileSystem.DirectoryExists(missionPath + "Old"))
                        {
                            DeleteOldMission(missionPath);
                        }
                        FileSystem.MoveDirectory(missionPath, missionPath + "Old");
                    }
                    FileSystem.CopyDirectory(Path.Combine(config.serverPath, "mpmissions", config.vanillaMissionName), missionPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }
        
        public void CopyExpansionFiles(string expansionTemplatePath, string missionPath)
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
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }
        #endregion CopyFunctions

        #region DownloadFunctions
        public string DownloadExpansionTemplates(Config config)
        {
            try
            {
                string expansionTemplatePath = string.Empty;
                if (FileSystem.FileExists(Path.Combine(config.gitInstallationPath, "bin", "git.exe")))
                {
                    if (FileSystem.DirectoryExists(config.expansionDownloadPath))
                    {
                        ProcessStartInfo gitInfo = new ProcessStartInfo();
                        gitInfo.CreateNoWindow = false;
                        gitInfo.FileName = Path.Combine(config.gitInstallationPath, "bin", "git.exe");
                        gitInfo.Arguments = "pull";
                        gitInfo.WorkingDirectory = Path.Combine(config.expansionDownloadPath);
                        Process gitProcess = new Process();
                        gitProcess.StartInfo = gitInfo;
                        gitProcess.Start();
                        gitProcess.WaitForExit();
                        gitProcess.Close();

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
                        string workingDirectory = string.Empty;
                        if (config.expansionDownloadPath == string.Empty)
                        {
                            config.expansionDownloadPath = Path.Combine(config.serverPath, "mpmissions", "DayZ-Expansion-Missions");
                            workingDirectory = Path.Combine(config.serverPath, "mpmissions");
                        }
                        else
                        {
                            workingDirectory = config.expansionDownloadPath.Remove(config.expansionDownloadPath.LastIndexOf("\\") - 1);
                        }
                        ProcessStartInfo gitInfo = new ProcessStartInfo();
                        gitInfo.CreateNoWindow = false;
                        gitInfo.FileName = Path.Combine(config.gitInstallationPath, "bin", "git.exe");
                        gitInfo.Arguments = "clone https://github.com/ExpansionModTeam/DayZ-Expansion-Missions.git";
                        gitInfo.WorkingDirectory = workingDirectory;
                        Process gitProcess = new Process();
                        gitProcess.StartInfo = gitInfo;
                        gitProcess.Start();
                        gitProcess.WaitForExit();
                        gitProcess.Close();

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
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                return string.Empty;
            }
        }
        #endregion DownloadFunctions

        #region DeleteFunctions
        public void DeleteOldMission(string missionPath)
        {
            try
            {
                FileSystem.DeleteDirectory(missionPath + "Old", DeleteDirectoryOption.DeleteAllContents);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }
        #endregion DeleteFunctions
    }
}
