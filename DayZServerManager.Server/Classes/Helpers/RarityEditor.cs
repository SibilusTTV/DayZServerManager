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
                UpdateHardlineAndTypes(Path.Combine(Manager.MISSION_PATH, Manager.managerConfig.missionName), name, rarityFile);
                UpdateRarities(Path.Combine(Manager.MISSION_PATH, Manager.managerConfig.missionName), name, rarityFile);
                UpdateRarities(Path.Combine(Manager.MISSION_PATH, Manager.managerConfig.missionTemplateName),name, rarityFile);
            }
        }

        private static void UpdateRarities(string folderPath, string name, RarityFile rarityFile)
        {
            JSONSerializer.SerializeJSONFile(Path.Combine(folderPath, name), rarityFile);
        }

        private static void UpdateHardlineAndTypes(string folderPath, string name, RarityFile rarityFile)
        {
            switch (name)
            {
                case "vanillaRarities.json":
                    UpdateVanillaTypes(folderPath, rarityFile);
                    break;
                case "customFilesRarities.json":
                    UpdateCustomTypes(folderPath, rarityFile);
                    break;
                case "expansionRarities.json":
                    UpdateExpansionTypes(folderPath, rarityFile);
                    break;
            }
            UpdateHardline(folderPath, rarityFile);
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
                        }
                    }
                }
            }
        }

        #region UpdateTypes
        private static void UpdateVanillaTypes(string folderPath, RarityFile rarityFile)
        {
            TypesFile? typesFile = GetVanillaTypesFile(folderPath);
            if (typesFile != null && Manager.managerConfig != null)
            {
                MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);
            }

            TypesChangesFile? typesChangesFile = GetVanillaTypesChangesFile(folderPath);
            if (typesChangesFile != null && typesFile != null)
            {
                MissionUpdater.UpdateTypesWithTypesChanges(typesFile, typesChangesFile);
            }
        }

        private static void UpdateExpansionTypes(string folderPath, RarityFile rarityFile)
        {
            TypesFile? typesFile = GetExpansionTypesFile(folderPath);
            if (typesFile != null)
            {
                MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);
            }

            TypesChangesFile? typesChangesFile = GetExpansionTypesChangesFile(folderPath);
            if (typesChangesFile != null && typesFile != null)
            {
                MissionUpdater.UpdateTypesWithTypesChanges(typesFile, typesChangesFile);
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
                }
            }
        }
        #endregion UpdateTypes

        #region GetTypesFiles
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

        private static TypesFile? GetVanillaTypesFile(string folderPath)
        {
            List<string> missionFolders = FileSystem.GetDirectories(folderPath).ToList<string>();
            if (missionFolders.Find(x => Path.GetFileName(x) == "db") != null)
            {
                List<string> missionExpansionFolders = FileSystem.GetFiles(Path.Combine(folderPath, "db")).ToList<string>();
                if (missionExpansionFolders.Find(x => Path.GetFileName(x) == "types.xml") != null)
                {
                    return JSONSerializer.DeserializeJSONFile<TypesFile>(Path.Combine(folderPath, "db", "types.xml"));
                }
            }
            return null;
        }

        private static TypesFile? GetExpansionTypesFile(string folderPath)
        {
            List<string> missionFolders = FileSystem.GetDirectories(folderPath).ToList<string>();
            if (missionFolders.Find(x => Path.GetFileName(x) == "expansion_ce") != null)
            {
                List<string> missionDbFiles = FileSystem.GetFiles(Path.Combine(folderPath, "expansion_ce")).ToList<string>();
                if (missionDbFiles.Find(x => Path.GetFileName(x) == "expansion_types.xml") != null)
                {
                    return XMLSerializer.DeserializeXMLFile<TypesFile>(Path.Combine(folderPath, "expansion_ce", "expansion_types.xml"));
                }
            }
            return null;
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
