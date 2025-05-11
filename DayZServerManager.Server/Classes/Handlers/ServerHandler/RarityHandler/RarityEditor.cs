using DayZServerManager.Server.Classes.Handlers.ServerHandler.MissionHandler;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityFile;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesChangesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.VisualBasic.FileIO;

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
            Logger.Info("Updating Rarity, Hardline and Types");

            UpdateRarities(Path.Combine(Manager.MPMISSIONS_PATH, Manager.managerConfig.missionTemplateName), name, rarityFile);
            UpdateHardlineAndTypes(Manager.MPMISSIONS_PATH, name, rarityFile);

            Logger.Info("Rarity, Hardline and Types updated");
        }

        private static void UpdateRarities(string missionFolder, string name, RarityFile rarityFile)
        {
            Logger.Info("Updating Rarities");
            JSONSerializer.SerializeJSONFile(Path.Combine(missionFolder, name), rarityFile);
            Logger.Info("Rarities Updated");
        }

        private static void UpdateHardlineAndTypes(string mpmissionsFolder, string name, RarityFile rarityFile)
        {
            Logger.Info("Updating Hardline and Types");
            switch (name)
            {
                case "vanillaRarities.json":
                    UpdateVanillaTypes(mpmissionsFolder, rarityFile);
                    break;
                case Manager.MISSION_EXPANSION_RARITIES_FILE_NAME:
                    UpdateExpansionTypes(mpmissionsFolder, rarityFile);
                    break;
                case "customFilesRarities.json":
                    UpdateCustomTypes(Path.Combine(mpmissionsFolder, Manager.managerConfig.missionName), rarityFile);
                    UpdateCustomTypes(Path.Combine(mpmissionsFolder, Manager.managerConfig.missionTemplateName), rarityFile);
                    break;
            }
            UpdateHardline(Path.Combine(mpmissionsFolder, Manager.managerConfig.missionName), Path.Combine(mpmissionsFolder, Manager.managerConfig.missionTemplateName), rarityFile);
            Logger.Info("Hardline and Types updated");
        }

        private static void UpdateHardline(string missionPath, string missionTemplatePath, RarityFile rarityFile)
        {
            try
            {
                HardlineFile? hardlineFile = JSONSerializer.DeserializeJSONFile<HardlineFile>(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME, Manager.MISSION_EXPANSION_HARDLINE_SETTINGS_FILE_NAME));
                RarityFile? vanillaRarity = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, Manager.MISSION_VANILLA_RARITIES_FILE_NAME));
                RarityFile? expansionRarity = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, Manager.MISSION_EXPANSION_RARITIES_FILE_NAME));
                RarityFile? customFilesRarityFile = JSONSerializer.DeserializeJSONFile<RarityFile>(Path.Combine(missionTemplatePath, Manager.MISSION_CUSTOM_FILES_RARITIES_FILE_NAME));

                if (hardlineFile != null)
                {
                    if (vanillaRarity != null)
                    {
                        MissionUpdater.UpdateHardlineRarity(hardlineFile, vanillaRarity);
                    }
                    if (expansionRarity != null)
                    {
                        MissionUpdater.UpdateHardlineRarity(hardlineFile, expansionRarity);
                    }
                    if (customFilesRarityFile != null)
                    {
                        MissionUpdater.UpdateHardlineRarity(hardlineFile, customFilesRarityFile);
                    }
                    JSONSerializer.SerializeJSONFile(Path.Combine(missionPath, Manager.MISSION_EXPANSION_FOLDER_NAME, Manager.MISSION_EXPANSION_SETTINGS_FOLDER_NAME, Manager.MISSION_EXPANSION_HARDLINE_SETTINGS_FILE_NAME), hardlineFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when updating RarityFile", ex);
            }
        }

        #region UpdateTypes
        private static void UpdateVanillaTypes(string mpmissionsPath, RarityFile rarityFile)
        {
            if (File.Exists(Path.Combine(mpmissionsPath, Manager.managerConfig.missionName, Manager.MISSION_DB_FOLDER_NAME, Manager.MISSION_TYPES_FILE_NAME)))
            {
                string typesPath = Path.Combine(mpmissionsPath, Manager.managerConfig.missionName, Manager.MISSION_DB_FOLDER_NAME, Manager.MISSION_TYPES_FILE_NAME);
                TypesFile? typesFile = XMLSerializer.DeserializeXMLFile<TypesFile>(typesPath);
                if (typesFile != null)
                {
                    MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);

                    TypesChangesFile? typesChangesFile = GetVanillaTypesChangesFile(Path.Combine(mpmissionsPath, Manager.managerConfig.missionTemplateName));
                    if (typesChangesFile != null)
                    {
                        MissionUpdater.UpdateTypesWithTypesChanges(typesFile, typesChangesFile);
                    }

                    XMLSerializer.SerializeXMLFile(typesPath, typesFile);
                }
            }
        }

        private static void UpdateExpansionTypes(string mpmissionsPath, RarityFile rarityFile)
        {
            if (File.Exists(Path.Combine(mpmissionsPath, Manager.managerConfig.missionName, Manager.MISSION_EXPANSIONCE_FOLDER_NAME, Manager.MISSION_EXPANSION_TYPES_FILE_NAME)))
            {
                string typesPath = Path.Combine(mpmissionsPath, Manager.managerConfig.missionName, Manager.MISSION_EXPANSIONCE_FOLDER_NAME, Manager.MISSION_EXPANSION_TYPES_FILE_NAME);
                TypesFile? typesFile = XMLSerializer.DeserializeXMLFile<TypesFile>(typesPath);
                if (typesFile != null)
                {
                    MissionUpdater.UpdateTypesWithRarity(typesFile, rarityFile);

                    TypesChangesFile? typesChangesFile = GetExpansionTypesChangesFile(Path.Combine(mpmissionsPath, Manager.managerConfig.missionTemplateName));
                    if (typesChangesFile != null)
                    {
                        MissionUpdater.UpdateTypesWithTypesChanges(typesFile, typesChangesFile);
                    }

                    XMLSerializer.SerializeXMLFile(typesPath, typesFile);
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
            if (File.Exists(Path.Combine(folderPath, Manager.MISSION_VANILLA_RARITIES_FILE_NAME)))
            {
                return JSONSerializer.DeserializeJSONFile<TypesChangesFile>(Path.Combine(folderPath, Manager.MISSION_VANILLA_RARITIES_FILE_NAME));
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
