using DayZServerManager.Server.Classes.Handlers.ServerHandler.MissionHandler;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityFile;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesChangesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;

namespace DayZServerManager.Server.Classes.Handlers.ServerHandler.RarityHandler
{
    public static class RarityEditor
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static RarityFile? GetRarityFile(string name)
        {
            if (File.Exists(Path.Combine(Manager.MPMISSIONS_PATH, Manager.managerConfig.missionTemplateName, name)))
            {
                return JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(Manager.MPMISSIONS_PATH, Manager.managerConfig.missionTemplateName, name));
            }
            return null;
        }

        public static void UpdateRaritiesAndTypes(string name, RarityFile rarityFile)
        {
            Logger.Info("Updating Rarity and Types");

            UpdateRarityFile(Path.Combine(Manager.MPMISSIONS_PATH, Manager.managerConfig.missionTemplateName), name, rarityFile);
            UpdateTypesFiles(Manager.MPMISSIONS_PATH, name, rarityFile);

            Manager.dayZServer.MissionNeedsUpdating = true;

            Logger.Info("Rarity and Types updated");
        }

        private static void UpdateRarityFile(string missionFolder, string name, RarityFile rarityFile)
        {
            Logger.Info("Updating Rarities");
            JSONSerializer.SerializeJSONFile(Path.Combine(missionFolder, name), rarityFile);
            Logger.Info("Rarities Updated");
        }

        private static void UpdateTypesFiles(string mpmissionsFolder, string name, RarityFile rarityFile)
        {
            if (name == Manager.MISSION_CUSTOM_FILES_RARITIES_FILE_NAME)
            {
                Logger.Info("Updating Types");
                UpdateCustomTypes(Path.Combine(mpmissionsFolder, Manager.managerConfig.missionTemplateName), rarityFile);
                Logger.Info("Types updated");
            }
        }

        #region UpdateTypes
        private static void UpdateCustomTypes(string folderPath, RarityFile rarityFile)
        {
            List<string> typesFilePaths = GetAllCustomTypesFiles(folderPath);
            foreach (string filePath in typesFilePaths)
            {
                TypesFile? typesFile = XMLSerializer.DeserializeXMLFile<TypesFile>(filePath);
                if (typesFile != null)
                {
                    MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);
                    XMLSerializer.SerializeXMLFile(filePath, typesFile);
                }
            }
        }
        #endregion UpdateTypes

        #region GetTypesFiles
        private static List<string> GetAllCustomTypesFiles(string folderPath)
        {
            List<string> typesFiles = new List<string>();
            if (File.Exists(Path.Combine(folderPath, Manager.MISSION_ECONOMYCORE_FILE_NAME)))
            {
                string economyCoreFilePath = Path.Combine(folderPath, Manager.MISSION_ECONOMYCORE_FILE_NAME);
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
            if (File.Exists(Path.Combine(folderPath, Manager.MISSION_EXPANSION_TYPES_CHANGES_FILE_NAME)))
            {
                return JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(folderPath, Manager.MISSION_EXPANSION_TYPES_CHANGES_FILE_NAME));
            }
            return null;
        }

        private static TypesChangesFile? GetVanillaTypesChangesFile(string folderPath)
        {
            if (File.Exists(Path.Combine(folderPath, Manager.MISSION_VANILLA_TYPES_CHANGES_FILE_NAME)))
            {
                return JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(folderPath, Manager.MISSION_VANILLA_TYPES_CHANGES_FILE_NAME));
            }
            return null;
        }
        #endregion GetTypesChanges
    }
}
