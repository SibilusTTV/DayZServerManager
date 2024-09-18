using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityFile;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesChangesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.VisualBasic.FileIO;

namespace DayZServerManager.Server.Classes.Helpers
{
    public static class RarityEditor
    {
        public static RarityFile? GetRarityFile(string name)
        {
            if (Manager.managerConfig != null)
            {
                List<string> missionFiles = FileSystem.GetFiles(Path.Combine(Manager.MISSION_PATH, Manager.managerConfig.missionTemplateName)).ToList<string>();
                if (missionFiles.Find(x => Path.GetFileName(x) == name) != null)
                {
                    return JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(Manager.MISSION_PATH, Manager.managerConfig.missionTemplateName, name));
                }
            }
            return null;
        }

        public static void UpdateRaritiesAndTypes(string name, RarityFile rarityFile)
        {
            if (Manager.managerConfig != null)
            {
                Manager.WriteToConsole("Updating Rarity, Hardline and Types");

                UpdateHardlineAndTypes(Manager.MISSION_PATH, name, rarityFile);
                UpdateRarities(Path.Combine(Manager.MISSION_PATH, Manager.managerConfig.missionTemplateName), name, rarityFile);

                Manager.WriteToConsole("Rarity, Hardline and Types updated");
            }
        }

        private static void UpdateRarities(string missionFolder, string name, RarityFile rarityFile)
        {
            Manager.WriteToConsole("Updating Rarities");
            JSONSerializer.SerializeJSONFile(Path.Combine(missionFolder, name), rarityFile);
            Manager.WriteToConsole("Rarities Updated");
        }

        private static void UpdateHardlineAndTypes(string mpmissionsFolder, string name, RarityFile rarityFile)
        {
            Manager.WriteToConsole("Updating Hardline and Types");
            switch (name)
            {
                case "vanillaRarities.json":
                    UpdateVanillaTypes(mpmissionsFolder, rarityFile);
                    break;
                case "expansionRarities.json":
                    UpdateExpansionTypes(mpmissionsFolder, rarityFile);
                    break;
                case "customFilesRarities.json":
                    if (Manager.managerConfig != null)
                    {
                        UpdateCustomTypes(Path.Combine(mpmissionsFolder, Manager.managerConfig.missionName), rarityFile);
                    }
                    break;
            }
            if (Manager.managerConfig != null)
            {
                UpdateHardline(Path.Combine(mpmissionsFolder, Manager.managerConfig.missionName), rarityFile);
            }
            Manager.WriteToConsole("Hardline and Types updated");
        }

        private static void UpdateHardline(string folderPath, RarityFile rarityFile)
        {
            List<string> missionFolders = FileSystem.GetDirectories(folderPath).ToList<string>();
            if (missionFolders.Find(x => Path.GetFileName(x) == "expansion") != null)
            {
                List<string> missionExpansionFolders = FileSystem.GetDirectories(Path.Combine(folderPath, "expansion")).ToList<string>();
                if (missionFolders.Find(x => Path.GetFileName(x) == "settings") != null)
                {
                    List<string> missionExpansionSettingsFiles = FileSystem.GetFiles(Path.Combine(folderPath, "expansion", "settings")).ToList<string>();
                    if (missionExpansionSettingsFiles.Find(x => Path.GetFileName(x) == "HardlineSettings.json") != null)
                    {
                        HardlineFile? hardlineFile = JSONSerializer.DeserializeJSONFile<HardlineFile>(Path.Combine(folderPath, "expansion", "settings", "HardlineSettings.json"));
                        if (hardlineFile != null)
                        {
                            MissionUpdater.UpdateHardlineRarity(hardlineFile, rarityFile);
                            JSONSerializer.SerializeJSONFile<HardlineFile>(Path.Combine(folderPath, "expansion", "settings", "HardlineSettings.json"), hardlineFile);
                        }
                    }
                }
            }
        }

        #region UpdateTypes
        private static void UpdateVanillaTypes(string mpmissionsPath, RarityFile rarityFile)
        {
            if (Manager.managerConfig != null)
            {
                string typesPath = GetVanillaTypesPath(Path.Combine(mpmissionsPath, Manager.managerConfig.missionName));
                TypesFile? typesFile = XMLSerializer.DeserializeXMLFile<TypesFile>(typesPath);
                if (typesFile != null)
                {
                    MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);

                    TypesChangesFile? typesChangesFile = GetVanillaTypesChangesFile(Path.Combine(mpmissionsPath, Manager.managerConfig.missionTemplateName));
                    if (typesChangesFile != null)
                    {
                        MissionUpdater.UpdateTypesWithTypesChanges(typesFile, typesChangesFile);
                    }

                    XMLSerializer.SerializeXMLFile<TypesFile>(typesPath, typesFile);
                }
            }
        }

        private static void UpdateExpansionTypes(string mpmissionsPath, RarityFile rarityFile)
        {
            if (Manager.managerConfig != null)
            {
                string typesPath = GetExpansionTypesPath(Path.Combine(mpmissionsPath, Manager.managerConfig.missionName));
                TypesFile? typesFile = XMLSerializer.DeserializeXMLFile<TypesFile>(typesPath);
                if (typesFile != null)
                {
                    MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);

                    TypesChangesFile? typesChangesFile = GetExpansionTypesChangesFile(Path.Combine(mpmissionsPath, Manager.managerConfig.missionTemplateName));
                    if (typesChangesFile != null)
                    {
                        MissionUpdater.UpdateTypesWithTypesChanges(typesFile, typesChangesFile);
                    }

                    XMLSerializer.SerializeXMLFile<TypesFile>(typesPath, typesFile);
                }
            }
        }

        private static void UpdateCustomTypes(string folderPath, RarityFile rarityFile)
        {
            List<string> typesFilePaths = GetAllCustomTypesFiles(folderPath);
            foreach (string filePath in typesFilePaths)
            {
                TypesFile? typesFile = XMLSerializer.DeserializeXMLFile<TypesFile>(filePath);
                if (typesFile != null)
                {
                    MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);
                    XMLSerializer.SerializeXMLFile<TypesFile>(filePath, typesFile);
                }
            }
        }
        #endregion UpdateTypes

        #region GetTypesFiles
        private static string GetVanillaTypesPath(string folderPath)
        {
            List<string> missionFolders = FileSystem.GetDirectories(folderPath).ToList<string>();
            if (missionFolders.Find(x => Path.GetFileName(x) == "db") != null)
            {
                List<string> missionDbFiles = FileSystem.GetFiles(Path.Combine(folderPath, "db")).ToList<string>();
                string? typesPath = missionDbFiles.Find(x => Path.GetFileName(x) == "types.xml");
                if (typesPath != null)
                {
                    return typesPath;
                }
            }
            return string.Empty;
        }

        private static string GetExpansionTypesPath(string folderPath)
        {
            List<string> missionFolders = FileSystem.GetDirectories(folderPath).ToList<string>();
            if (missionFolders.Find(x => Path.GetFileName(x) == "expansion_ce") != null)
            {
                List<string> missionExpansionFiles = FileSystem.GetFiles(Path.Combine(folderPath, "expansion_ce")).ToList<string>();
                string? expansionTypesPath = missionExpansionFiles.Find(x => Path.GetFileName(x) == "expansion_types.xml");
                if (expansionTypesPath != null)
                {
                    return expansionTypesPath;
                }
            }
            return string.Empty;
        }

        private static List<string> GetAllCustomTypesFiles(string folderPath)
        {
            List<string> typesFiles = new List<string>();
            List<string> missionFiles = FileSystem.GetFiles(folderPath).ToList<string>();
            if (missionFiles.Find(x => Path.GetFileName(x) == "cfgeconomycore.xml") != null)
            {
                string economyCoreFilePath = Path.Combine(folderPath, "cfgeconomycore.xml");
                EconomyCoreFile? economyCoreFile = XMLSerializer.DeserializeXMLFile<EconomyCoreFile>(economyCoreFilePath);
                if (economyCoreFile != null)
                {
                    foreach (CeItem ceItem in economyCoreFile.ceItems)
                    {
                        foreach (FileItem fileItem in ceItem.fileItems)
                        {
                            if (fileItem.type == "types")
                            {
                                typesFiles.Add(Path.Combine(folderPath, ceItem.folder, fileItem.name));
                            }
                        }
                    }
                }
            }
            return typesFiles;
        }
        #endregion GetTypesFiles

        #region GetTypesChanges
        private static TypesChangesFile? GetExpansionTypesChangesFile(string folderPath)
        {
            List<string> missionFiles = FileSystem.GetFiles(folderPath).ToList<string>();
            if (missionFiles.Find(x => Path.GetFileName(x) == "expansionTypesChanges.json") != null)
            {
                return JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(folderPath, "expansionTypesChanges.json"));
            }
            return null;
        }

        private static TypesChangesFile? GetVanillaTypesChangesFile(string folderPath)
        {
            List<string> missionFiles = FileSystem.GetFiles(folderPath).ToList<string>();
            if (missionFiles.Find(x => Path.GetFileName(x) == "vanillaTypesChanges.json") != null)
            {
                return JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(folderPath, "vanillaTypesChanges.json"));
            }
            return null;
        }
        #endregion GetTypesChanges
    }
}
