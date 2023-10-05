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
                string missionTemplatePath = Path.Combine(config.serverPath, "mpmissions", config.missionTemplateName);
                string expansionTemplatePath = Path.Combine(config.serverPath, "mpmissions", config.expansionTemplateName, "Template", config.mapName);

                // Rename the old mission folder and copy the contents of the vanilla folder
                if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, "mpmissions", config.vanillaMissionName)))
                {
                    if (FileSystem.DirectoryExists(missionPath))
                    {
                        if (FileSystem.DirectoryExists(missionPath + "Old"))
                        {
                            FileSystem.DeleteDirectory(missionPath + "Old", DeleteDirectoryOption.DeleteAllContents);
                        }
                        FileSystem.MoveDirectory(missionPath, missionPath + "Old");
                    }
                    FileSystem.CopyDirectory(Path.Combine(config.serverPath, "mpmissions", config.vanillaMissionName), missionPath);
                }

                // Change the variables in the globals.xml of TimeLogin to 5 and ZombieMaxCount to 500
                if (FileSystem.FileExists(Path.Combine(missionPath, "db", "globals.xml")))
                {
                    GlobalsFile? globals;
                    using (StreamReader reader = new StreamReader(Path.Combine(missionPath, "db", "globals.xml")))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(GlobalsFile));
                        globals = (GlobalsFile?)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    if (globals != null)
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
                        using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "db", "globals.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(GlobalsFile));
                            serializer.Serialize(writer, globals);
                            writer.Close();
                        }
                    }
                }

                // Get the new expansion mission template from git
                if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, "mpmissions", config.expansionTemplateName)) && FileSystem.FileExists(Path.Combine(config.gitInstallationPath, "bin", "git.exe")))
                {
                    ProcessStartInfo gitInfo = new ProcessStartInfo();
                    gitInfo.CreateNoWindow = false;
                    gitInfo.FileName = Path.Combine(config.gitInstallationPath, "bin", "git.exe");
                    gitInfo.Arguments = "pull";
                    gitInfo.WorkingDirectory = Path.Combine(config.serverPath, "mpmissions", config.expansionTemplateName);
                    Process gitProcess = new Process();
                    gitProcess.StartInfo = gitInfo;
                    gitProcess.Start();
                    gitProcess.WaitForExit();
                    gitProcess.Close();
                }

                // Add the other parts of the cfgeconomycore.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                if (FileSystem.FileExists(Path.Combine(missionPath, "cfgeconomycore.xml")))
                {
                    EconomyCoreFile? missionEconomyCore;
                    using (StreamReader reader = new StreamReader(Path.Combine(missionPath, "cfgeconomycore.xml")))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                        missionEconomyCore = (EconomyCoreFile?)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    if (FileSystem.FileExists(Path.Combine(expansionTemplatePath, "cfgeconomycore.xml")))
                    {
                        EconomyCoreFile? expansionTemplateEconomyCore;
                        using (StreamReader reader = new StreamReader(Path.Combine(expansionTemplatePath, "cfgeconomycore.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                            expansionTemplateEconomyCore = (EconomyCoreFile?)serializer.Deserialize(reader);
                            reader.Close();
                        }

                        if (expansionTemplateEconomyCore != null)
                        {
                            if (missionEconomyCore != null)
                            {
                                foreach (CeItem ceItem in expansionTemplateEconomyCore.ceItems)
                                {
                                    CeItem? ceItemInMission = SearchForCeItem(ceItem, missionEconomyCore);
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
                                        missionEconomyCore.ceItems.Add(ceItem);
                                    }
                                }
                            }
                            else
                            {
                                missionEconomyCore = expansionTemplateEconomyCore;
                            }
                        }
                    }

                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "cfgeconomycore.xml")))
                    {
                        EconomyCoreFile? missionTemplateEconomyCore;
                        using (StreamReader reader = new StreamReader(Path.Combine(missionTemplatePath, "cfgeconomycore.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                            missionTemplateEconomyCore = (EconomyCoreFile?)serializer.Deserialize(reader);
                            reader.Close();
                        }

                        if (missionTemplateEconomyCore != null)
                        {
                            if (missionEconomyCore != null)
                            {
                                foreach (CeItem ceItem in missionTemplateEconomyCore.ceItems)
                                {
                                    CeItem? ceItemInMission = SearchForCeItem(ceItem, missionEconomyCore);
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
                                        missionEconomyCore.ceItems.Add(ceItem);
                                    }
                                }
                            }
                            else
                            {
                                missionEconomyCore = missionTemplateEconomyCore;
                            }
                        }
                    }

                    if (missionEconomyCore != null)
                    {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "cfgeconomycore.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EconomyCoreFile));
                            serializer.Serialize(writer, missionEconomyCore);
                            writer.Close();
                        }
                    }
                }

                // Add the other parts of the cfgeventspawns.xml from the expansionTemplate and the missionTemplate to the one from the new mission folder
                if (FileSystem.FileExists(Path.Combine(missionPath, "cfgeventspawns.xml")))
                {
                    EventSpawnsFile? missionEventSpawns;
                    using (StreamReader reader = new StreamReader(Path.Combine(missionPath, "cfgeventspawns.xml")))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                        missionEventSpawns = (EventSpawnsFile?)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    if (FileSystem.FileExists(Path.Combine(expansionTemplatePath, "cfgeventspawns.xml")))
                    {
                        EventSpawnsFile? expansionTemplateEventSpawns;
                        using (StreamReader reader = new StreamReader(Path.Combine(expansionTemplatePath, "cfgeventspawns.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                            expansionTemplateEventSpawns = (EventSpawnsFile?)serializer.Deserialize(reader);
                            reader.Close();
                        }

                        if (expansionTemplateEventSpawns != null)
                        {
                            if (missionEventSpawns != null)
                            {
                                foreach (EventItem eventItem in expansionTemplateEventSpawns.eventItems)
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
                            else
                            {
                                missionEventSpawns = expansionTemplateEventSpawns;
                            }
                        }
                    }

                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "cfgeventspawns.xml")))
                    {
                        EventSpawnsFile? missionTemplateEventSpawns;
                        using (StreamReader reader = new StreamReader(Path.Combine(missionTemplatePath, "cfgeventspawns.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                            missionTemplateEventSpawns = (EventSpawnsFile?)serializer.Deserialize(reader);
                            reader.Close();
                        }

                        if (missionTemplateEventSpawns != null)
                        {
                            if (missionEventSpawns != null)
                            {
                                foreach (EventItem eventItem in missionTemplateEventSpawns.eventItems)
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
                            else
                            {
                                missionEventSpawns = missionTemplateEventSpawns;
                            }
                        }
                    }

                    if (missionEventSpawns != null)
                    {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "cfgeventspawns.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EventSpawnsFile));
                            serializer.Serialize(writer, missionEventSpawns);
                            writer.Close();
                        }
                    }
                }

                // Add the part of the main method of the init.c of the missionTemplate to the one from the new mission folder
                if (FileSystem.FileExists(Path.Combine(missionPath, "init.c")) && FileSystem.FileExists(Path.Combine(missionTemplatePath, "init.c")))
                {
                    string missionInit;
                    string templateInit;
                    using (StreamReader reader = new StreamReader(Path.Combine(missionPath, "init.c")))
                    {
                        missionInit = reader.ReadToEnd();
                        reader.Close();
                    }
                    using (StreamReader reader = new StreamReader(Path.Combine(missionTemplatePath, "init.c")))
                    {
                        templateInit = reader.ReadToEnd();
                        reader.Close();
                    }

                    missionInit = missionInit.Insert(missionInit.IndexOf("{") + 1, templateInit.Substring(templateInit.IndexOf("{") + 1, templateInit.IndexOf("}") - templateInit.IndexOf("{") - 2));

                    using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "init.c")))
                    {
                        writer.Write(missionInit);
                        writer.Close();
                    }
                }

                // Copy the folder expansion_ce from the expansionTemplate to the new mission folder
                if (FileSystem.DirectoryExists(expansionTemplatePath))
                {
                    FileSystem.CopyDirectory(Path.Combine(expansionTemplatePath, "expansion_ce"), Path.Combine(missionPath, "expansion_ce"), true);
                }
                else if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, "mpmissions", config.expansionTemplateName, "Template", "0_INCOMPLETE", config.mapName)))
                {
                    expansionTemplatePath = Path.Combine(config.serverPath, "mpmissions", config.expansionTemplateName, "Template", "0_INCOMPLETE", config.mapName);
                    FileSystem.CopyDirectory(Path.Combine(expansionTemplatePath, "expansion_ce"), Path.Combine(missionPath, "expansion_ce"), true);
                }

                // Copy the folders CustomFiles and expansion and also the files mapgrouppos.xml, cfgweather.xml and cfgplayerspawnpoints.xml from the missionTemplate to the new mission folder
                if (FileSystem.DirectoryExists(missionTemplatePath) && FileSystem.DirectoryExists(missionPath))
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

                // Changing the types files to reflect the rarities
                if (FileSystem.DirectoryExists(Path.Combine(missionTemplatePath)))
                {
                    RarityFile? hardlineRarity = null;
                    if (FileSystem.FileExists(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json")))
                    {
                        using (StreamReader reader = new StreamReader(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json")))
                        {
                            string json = reader.ReadToEnd();
                            hardlineRarity = JsonSerializer.Deserialize<RarityFile>(json);
                            reader.Close();
                        }
                    }

                    TypesFile? vanillaTypes = null;
                    if (FileSystem.FileExists(Path.Combine(missionPath, "db", "types.xml")))
                    {
                        using (StreamReader reader = new StreamReader(Path.Combine(missionPath, "db", "types.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                            vanillaTypes = (TypesFile?)serializer.Deserialize(reader);
                            reader.Close();
                        }
                    }

                    TypesFile? expansionTypes = null;
                    if (FileSystem.FileExists(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml")))
                    {
                        using (StreamReader reader = new StreamReader(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                            expansionTypes = (TypesFile?)serializer.Deserialize(reader);
                            reader.Close();
                        }
                    }

                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "vanillaRarities.json")))
                    {
                        RarityFile? vanillaRarity = null;
                        using (StreamReader reader = new StreamReader(Path.Combine(missionTemplatePath, "vanillaRarities.json")))
                        {
                            string json = reader.ReadToEnd();
                            vanillaRarity = JsonSerializer.Deserialize<RarityFile>(json);
                            reader.Close();
                        }

                        if (vanillaRarity != null)
                        {
                            List<string> vanillaRarityKeys = vanillaRarity.ItemRarity.Keys.ToList();

                            if (hardlineRarity != null)
                            {
                                foreach (string key in vanillaRarityKeys)
                                {
                                    if (hardlineRarity.ItemRarity.ContainsKey(key.ToLower()))
                                    {
                                        hardlineRarity.ItemRarity[key.ToLower()] = vanillaRarity.ItemRarity[key];
                                    }
                                    else if (hardlineRarity.ItemRarity.ContainsKey(key))
                                    {
                                        hardlineRarity.ItemRarity[key] = vanillaRarity.ItemRarity[key];
                                    }
                                    else
                                    {
                                        hardlineRarity.ItemRarity.Add(key, vanillaRarity.ItemRarity[key]);
                                    }
                                }
                            }

                            if (vanillaTypes != null)
                            {
                                foreach (string key in vanillaRarityKeys)
                                {
                                    TypesItem? item = SearchForTypesItem(key, vanillaTypes);
                                    if (item != null)
                                    {
                                        switch (vanillaRarity.ItemRarity[key])
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
                        }
                    }

                    // Change the Lifetimes of items in the types.xml
                    if (vanillaTypes != null)
                    {
                        if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "TypesChanges.json")))
                        {
                            TypesChangesFile? changes = null;
                            using (StreamReader reader = new StreamReader(Path.Combine(missionTemplatePath, "TypesChanges.json")))
                            {
                                string json = reader.ReadToEnd();
                                changes = JsonSerializer.Deserialize<TypesChangesFile>(json);
                                reader.Close();
                            }

                            if (changes != null)
                            {
                                foreach (TypesChangesItem change in changes.types) 
                                {
                                    TypesItem? item = SearchForTypesItem(change.Name, vanillaTypes);
                                    if (item != null)
                                    {
                                        item.lifetime = change.Lifetime;
                                    }
                                }
                            }
                        }

                        using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "db", "types.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                            serializer.Serialize(writer, vanillaTypes);
                            writer.Close();
                        }
                    }

                    if (FileSystem.FileExists(Path.Combine(missionTemplatePath, "expansionRarities.json")))
                    {
                        RarityFile? expansionRarity = null;
                        using (StreamReader reader = new StreamReader(Path.Combine(missionTemplatePath, "expansionRarities.json")))
                        {
                            string json = reader.ReadToEnd();
                            expansionRarity = JsonSerializer.Deserialize<RarityFile>(json);
                            reader.Close();
                        }

                        if (expansionRarity != null)
                        {
                            List<string> expansionRarityKeys = expansionRarity.ItemRarity.Keys.ToList();

                            if (hardlineRarity != null)
                            {
                                foreach (string key in expansionRarityKeys)
                                {
                                    if (hardlineRarity.ItemRarity.ContainsKey(key.ToLower()))
                                    {
                                        hardlineRarity.ItemRarity[key.ToLower()] = expansionRarity.ItemRarity[key];
                                    }
                                    else if (hardlineRarity.ItemRarity.ContainsKey(key))
                                    {
                                        hardlineRarity.ItemRarity[key] = expansionRarity.ItemRarity[key];
                                    }
                                    else
                                    {
                                        hardlineRarity.ItemRarity.Add(key, expansionRarity.ItemRarity[key]);
                                    }
                                }

                                hardlineRarity.ShowHardlineHUD = expansionRarity.ShowHardlineHUD;
                                hardlineRarity.UseHumanity = expansionRarity.UseHumanity;
                                hardlineRarity.EnableItemRarity = expansionRarity.EnableItemRarity;
                            }

                            if (expansionTypes != null)
                            {
                                foreach (string key in expansionRarityKeys)
                                {
                                    TypesItem? item = SearchForTypesItem(key, expansionTypes);
                                    if (item != null)
                                    {
                                        switch (expansionRarity.ItemRarity[key])
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
                        }
                    }

                    if (expansionTypes != null)
                    {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "expansion_ce", "expansion_types.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(TypesFile));
                            serializer.Serialize(writer, expansionTypes);
                            writer.Close();
                        }
                    }

                    if (hardlineRarity != null)
                    {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(missionPath, "expansion", "settings", "HardlineSettings.json")))
                        {
                            JsonSerializerOptions options = new JsonSerializerOptions();
                            options.WriteIndented = true;
                            string json = JsonSerializer.Serialize(hardlineRarity, options);
                            writer.Write(json);
                            writer.Close();
                        }
                    }
                }

                // Copy over the data and map from the old mission into the new one
                if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, "mpmissions", config.missionName + "Old", "storage_1")))
                {
                    FileSystem.CopyDirectory(Path.Combine(config.serverPath, "mpmissions", config.missionName + "Old", "storage_1"), Path.Combine(missionPath, "storage_1"));
                }

                // Delete the old mission
                if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, "mpmissions", config.missionName + "Old")))
                {
                    FileSystem.DeleteDirectory(Path.Combine(config.serverPath, "mpmissions", config.missionName + "Old"), DeleteDirectoryOption.DeleteAllContents);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

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
    }
}
