using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.Helpers.Property;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using System.Text.Json;
using DayZServerManager.Server.Classes.Handlers.ServerHandler;
using DayZServerManager.Server.Classes.Handlers.SteamCMDHandler;
using System.Text;
using DayZServerManager.Server.Classes.Handlers.SchedulerHandler;
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace DayZServerManager.Server.Classes
{
    internal static class Manager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static ManagerConfig managerConfig = new ManagerConfig();

        #region Constants
        public const string MANAGER_CONFIG_NAME = "config.json";
        public const string SERVER_PATH = "server";
        public static string PROFILE_PATH = Path.Combine(SERVER_PATH, managerConfig.profileName);
        public const string SERVER_DEPLOY = "deploy";
        public static readonly string SERVER_EXECUTABLE = OperatingSystem.IsWindows() ? "DayZServer_x64.exe" : "DayZServer";
        public const string STEAM_CMD_PATH = "steamcmd";
        public static readonly string STEAM_CMD_EXECUTABLE = OperatingSystem.IsWindows() ? "steamcmd.exe" : "steamcmd.sh";
        public static readonly string STEAM_CMD_ZIP_NAME = OperatingSystem.IsWindows() ? "steamcmd.zip" : "steamcmd.tar.gz";
        public const string STEAMCMD_DOWNLOAD_URL = "https://steamcdn-a.akamaihd.net/client/installer/";
        public const string STEAMCMD_TAR_FILE_NAME = "steamcmd.tar";
        public const string MODS_PATH = "mods";
        public static readonly string WORKSHOP_PATH = Path.Combine("steamapps", "workshop", "content", "221100");
        public static readonly string MPMISSIONS_PATH = Path.Combine(SERVER_PATH, "mpmissions");
        public static readonly string EXPANSION_DOWNLOAD_PATH = Path.Combine(MPMISSIONS_PATH, "DayZ-Expansion-Missions");
        public static readonly string BATTLEYE_FOLDER_NAME = OperatingSystem.IsWindows() ? "BattlEye" : "battleye";
        public static readonly string BATTLEYE_CONFIG_NAME = OperatingSystem.IsWindows() ? "BEServer_x64.cfg" : "beserver_x64.cfg";
        public static string BATTLEYE_FOLDER_PATH = OperatingSystem.IsWindows() ? Path.Combine(PROFILE_PATH, BATTLEYE_FOLDER_NAME) : Path.Combine(SERVER_PATH, BATTLEYE_FOLDER_NAME);
        public const string BATTLEYE_BANS_NAME = "bans.txt";
        public const string SCHEDULER_DOWNLOAD_URL = "https://github.com/SibilusTTV/DayZScheduler/releases/latest/download/";
        public static readonly string SCHEDULER_ZIP_NAME = OperatingSystem.IsWindows() ? "windows.zip" : "linux.zip";
        public const string SCHEDULER_PATH = "scheduler";
        public const string SCHEDULER_CONFIG_NAME = "config.json";
        public static readonly string SCHEDULER_EXECUTABLE = OperatingSystem.IsWindows() ? "DayZScheduler.exe" : "DayZScheduler";
        public const int DAYZ_SERVER_BRANCH = 223350;
        public const int DAYZ_GAME_BRANCH = 221100;
        public const string PLAYER_DATABASE_NAME = "players_db.json";

        public const string PERSISTANCE_FOLDER_NAME = "storage_1";
        public const string BACKUP_DATA_FOLDER_NAME = "data";
        public const string BACKUP_LOGS_FOLDER_NAME = "logs";
        public const string MISSION_EXPANSIONCE_FOLDER_NAME = "expansion_ce";
        public const string MISSION_EXPANSION_TYPES_FILE_NAME = "expansion_types.xml";
        public const string MISSION_CUSTOM_FILE_NAME = "CustomFiles";
        public const string MISSION_EXAMPLE_TYPES_FILE_NAME = "exampleTypesFile.xml";
        public const string MISSION_EXAMPLE_MOD_FILES_FOLDER_NAME = "ExampleModFiles";
        public const string MISSION_DB_FOLDER_NAME = "db";
        public const string MISSION_TYPES_FILE_NAME = "types.xml";
        public const string MISSION_GLOBALS_FILE_NAME = "globals.xml";
        public const string MISSION_ECONOMYCORE_FILE_NAME = "cfgeconomycore.xml";
        public const string MISSION_EVENTSPAWNS_FILE_NAME = "cfgeventspawns.xml";
        public const string MISSION_ENVIRONMENTS_FILE_NAME = "cfgenvironment.xml";
        public const string MISSION_CUSTOM_FILES_RARITIES_FILE_NAME = "customFilesRarities.json";
        public const string MISSION_EXPANSION_RARITIES_FILE_NAME = "expansionRarities.json";
        public const string MISSION_VANILLA_RARITIES_FILE_NAME = "vanillaRarities.json";
        public const string MISSION_EXPANSION_TYPES_CHANGES_FILE_NAME = "expansionTypesChanges.json";
        public const string MISSION_VANILLA_TYPES_CHANGES_FILE_NAME = "vanillaTypesChanges.json";
        public const string MISSION_INIT_FILE_NAME = "init.c";
        public const string BACKUPS_FULL_MISSION_BACKUPS_FOLDER_NAME = "FullMissionBackups";
        public const string MISSION_EXPANSION_FOLDER_NAME = "expansion";
        public const string EXPANSION_MOD_SEARCH = "expansion";
        public const string MISSION_EXPANSION_SETTINGS_FOLDER_NAME = "settings";
        public const string PROFILE_EXPANSION_SETTINGS_FOLDER_NAME = "Settings";
        public const string PROFILE_EXPANSIONMOD_FOLDER_NAME = "ExpansionMod";
        public const string PROFILE_EXPANSION_NOTIFICATION_SCHEDULER_SETTINGS_FILE_NAME = "NotificationSchedulerSettings.json";
        public const string BANS_FILE_NAME = "bans.txt";
        public const string BAN_FILE_NAME = "ban.txt";
        public const string WHITELIST_FILE_NAME = "whitelist.txt";
        public const string DAYZ_SETTINGS_FILE_NAME = "dayzsetting.xml";
        public const string MISSION_EXPANSION_HARDLINE_SETTINGS_FILE_NAME = "HardlineSettings.json";
        public const string STEAMCMD_ANONYMOUS_LOGIN = "anonymous";
        public const string LOCALHOST = "127.0.0.1";
        public const string ADMIN_LOG_NAME = "DayZServer.ADM";
        public const string ADMIN_LOG_X64_NAME = "DayZServer_x64.ADM";

        // Mission rarity to numbers
        public const int EXOTIC_NOMINAL = 1;
        public const int EXOTIC_MINIMAL = 1;
        public const int MYTHIC_NOMINAL = 2;
        public const int MYTHIC_MINIMAL = 1;
        public const int LEGENDARY_NOMINAL = 5;
        public const int LEGENDARY_MINIMAL = 2;
        public const int EPIC_NOMINAL = 10;
        public const int EPIC_MINIMAL = 5;
        public const int RARE_NOMINAL = 20;
        public const int RARE_MINIMAL = 10;
        public const int UNCOMMON_NOMINAL = 40;
        public const int UNCOMMON_MINIMAL = 20;
        public const int COMMON_NOMINAL = 80;
        public const int COMMON_MINIMAL = 40;
        public const int POOR_NOMINAL = 160;
        public const int POOR_MINIMAL = 80;

        public static List<string> LogsNames = new()
        {
            ".ADM",
            ".RPT",
            ".log",
            ".mdmp"
        };

        public const string STATUS_CREDENTIALS = "Credentials";
        public const string STATUS_STARTING_SERVER = "Starting Server";
        public const string STATUS_STARTING_SCHEDULER = "Starting Scheduler";
        public const string STATUS_LISTENING = "Listening";
        public const string STATUS_STOPPING_SERVER = "Stopping Server";
        public const string STATUS_SERVER_STOPPED = "Server Stopped";
        public const string STATUS_STARTED = "Started";
        public const string STATUS_NOT_RUNNING = "Not Running";
        public const string STATUS_RUNNING = "Running";
        public const string STATUS_UPDATING_SCHEDULER = "Updating Scheduler";
        public const string STATUS_SCHEDULER_UPDATED = "Scheduler Updated";
        public const string STATUS_UPDATING_SERVER = "Updating Server";
        public const string STATUS_SERVER_UPDATED = "Server Updated";
        public const string STATUS_MOVING_SERVER = "Moving Server";
        public const string STATUS_SERVER_MOVED = "Server Moved";
        public const string STATUS_UPDATING_MISSION = "Updating Server";
        public const string STATUS_MISSION_UPDATED = "Server Updated";
        public const string STATUS_UPDATING_MODS = "Updating Mods";
        public const string STATUS_MODS_UPDATED = "Mods Updated";
        public const string STATUS_MOVING_MODS = "Moving Mods";
        public const string STATUS_MODS_MOVED = "Mods Moved";
        public const string STATUS_BACKING_UP_SERVER = "Backing up Server";
        public const string STATUS_SERVER_BACKED_UP = "Server Backed up";
        public const string STATUS_ERROR = "Error";
        public const string STATUS_CLIENT_CONFIG = "Client Config";
        public const string STATUS_STEAM_GUARD = "Steam Guard";
        public const string STATUS_CACHED_CREDENTIALS = "Cached Credentials";
        #endregion Constants

        public static ServerManager dayZServer = new ServerManager(Path.Combine(SERVER_PATH, managerConfig.serverConfigName));
        public static SchedulerManager scheduler = new SchedulerManager(LOCALHOST, managerConfig.RConPort, managerConfig.RConPassword, managerConfig.restartInterval, false, managerConfig.customMessages);
        public static Task? serverLoop;
        public static ManagerProps props = new ManagerProps(string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty);
        public static bool kill = false;
        public static string managerLog = "";

        private static Task? connectTask;
        private static Task? serverUpdateTask;

        public static void InitiateManager()
        {
            LoadManagerConfig();

            PROFILE_PATH = Path.Combine(SERVER_PATH, managerConfig.profileName);
            BATTLEYE_FOLDER_PATH = OperatingSystem.IsWindows() ? Path.Combine(PROFILE_PATH, BATTLEYE_FOLDER_NAME) : Path.Combine(SERVER_PATH, BATTLEYE_FOLDER_NAME);

            props = new ManagerProps(STATUS_LISTENING, STATUS_NOT_RUNNING, STATUS_NOT_RUNNING, 0, string.Empty, string.Empty);

            if (!Directory.Exists(SERVER_PATH))
            {
                Directory.CreateDirectory(SERVER_PATH);
            }

            if (!Directory.Exists(STEAM_CMD_PATH))
            {
                Directory.CreateDirectory(STEAM_CMD_PATH);
            }

            if (!Directory.Exists(MODS_PATH))
            {
                Directory.CreateDirectory(MODS_PATH);
            }

            if (!Directory.Exists(SCHEDULER_PATH))
            {
                Directory.CreateDirectory(SCHEDULER_PATH);
            }

            if (!Directory.Exists(SERVER_DEPLOY))
            {
                Directory.CreateDirectory(SERVER_DEPLOY);
            }

            if (!Directory.Exists(Path.Combine(SERVER_PATH, managerConfig.profileName)))
            {
                Directory.CreateDirectory(Path.Combine(SERVER_PATH, managerConfig.profileName));
            }

            if (!Directory.Exists(BATTLEYE_FOLDER_PATH))
            {
                Directory.CreateDirectory(BATTLEYE_FOLDER_PATH);
            }

            if (File.Exists(Path.Combine(BATTLEYE_FOLDER_PATH, BATTLEYE_BANS_NAME)))
            {
                using (FileStream fs = File.Create(Path.Combine(BATTLEYE_FOLDER_PATH, BATTLEYE_BANS_NAME))){
                    
                }

            }

            dayZServer = new ServerManager(Path.Combine(SERVER_PATH, managerConfig.serverConfigName));

            dayZServer.AdjustServerConfig(managerConfig.missionName, managerConfig.instanceId, managerConfig.steamQueryPort);
            dayZServer.SaveServerConfig(Path.Combine(SERVER_PATH, managerConfig.serverConfigName));

            if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
            {
                props.managerStatus = STATUS_CREDENTIALS;
                return;
            }

            if (managerConfig.autoStartServer)
            {
                Task task = new Task(() => { StartServer(); });
                task.Start();
            }
        }

        public static void StartServer()
        {
            PROFILE_PATH = Path.Combine(SERVER_PATH, managerConfig.profileName);
            BATTLEYE_FOLDER_PATH = OperatingSystem.IsWindows() ? Path.Combine(PROFILE_PATH, BATTLEYE_FOLDER_NAME) : Path.Combine(SERVER_PATH, BATTLEYE_FOLDER_NAME);
            
            if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
            {
                props.managerStatus = STATUS_CREDENTIALS;
                return;
            }

            dayZServer.AdjustServerConfig(managerConfig.missionName, managerConfig.instanceId, managerConfig.steamQueryPort);
            dayZServer.SaveServerConfig(Path.Combine(SERVER_PATH, managerConfig.serverConfigName));

            dayZServer.UpdateAndBackupServer(true, true);

            kill = false;
            props.managerStatus = STATUS_STARTING_SERVER;
            dayZServer.StartServer();

            props.managerStatus = STATUS_STARTING_SCHEDULER;
            StartScheduler();

            props.managerStatus = STATUS_LISTENING;
            props.dayzServerStatus = STATUS_STARTED;

            serverLoop = new Task(() => { StartServerLoop(); });
            serverLoop.Start();
        }

        private static void StartServerLoop()
        {
            Thread.Sleep(10000);

            double i = 10;
            while (!kill)
            {
                if (!dayZServer.CheckServer())
                {
                    Thread.Sleep(10000);
                    i += 10;

                    PROFILE_PATH = Path.Combine(SERVER_PATH, managerConfig.profileName);
                    BATTLEYE_FOLDER_PATH = OperatingSystem.IsWindows() ? Path.Combine(PROFILE_PATH, BATTLEYE_FOLDER_NAME) : Path.Combine(SERVER_PATH, BATTLEYE_FOLDER_NAME);

                    dayZServer.AdjustServerConfig(managerConfig.missionName, managerConfig.instanceId, managerConfig.steamQueryPort);
                    dayZServer.SaveServerConfig(Path.Combine(SERVER_PATH, managerConfig.serverConfigName));

                    dayZServer.UpdateAndBackupServer(false, true);

                    dayZServer.StartServer();
                }
                else
                {
                    props.adminLog = GetAdminLog();
                    int players = scheduler.GetPlayers();
                    Logger.Info($"The Server is still running with {players} players playing on it");
                }

                if (!CheckScheduler())
                {
                    scheduler.KillAutomaticTasks();
                    scheduler.KillCustomTasks();
                    StartScheduler();
                }
                else
                {
                    Logger.Info("Scheduler is still running");
                }

                if (i % 300 == 0 && (serverUpdateTask == null || serverUpdateTask.IsCompleted))
                {
                    serverUpdateTask = new Task(() => {
                        dayZServer.UpdateAndBackupServer(true, false);
                        dayZServer.RestartForUpdates();
                    });
                    serverUpdateTask.Start();
                }
                i += 10;
                props.managerStatus = STATUS_LISTENING;
                Thread.Sleep(10000);
            }

            Logger.Info(STATUS_STOPPING_SERVER);

            KillServerProcesses();

            props.managerStatus = STATUS_LISTENING;

            Logger.Info("Server was stopped");
        }

        public static void StopServer()
        {
            if (serverLoop != null)
            {
                kill = true;
                props.managerStatus = STATUS_STOPPING_SERVER;
            }
            else
            {
                KillServerProcesses();
            }
        }

        public static void KillServerProcesses()
        {
            try
            {
                if (scheduler != null)
                {
                    scheduler.KillAutomaticTasks();
                    scheduler.KillCustomTasks();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when killing scheduler",ex);
            }

            try
            {
                if (dayZServer.CheckServer())
                {
                    dayZServer.KillServerProcess();

                    Thread.Sleep(10000);

                    PROFILE_PATH = Path.Combine(SERVER_PATH, managerConfig.profileName);
                    BATTLEYE_FOLDER_PATH = OperatingSystem.IsWindows() ? Path.Combine(PROFILE_PATH, BATTLEYE_FOLDER_NAME) : Path.Combine(SERVER_PATH, BATTLEYE_FOLDER_NAME);

                    dayZServer.AdjustServerConfig(managerConfig.missionName, managerConfig.instanceId, managerConfig.steamQueryPort);
                    dayZServer.SaveServerConfig(Path.Combine(SERVER_PATH, managerConfig.serverConfigName));

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when killing server and ajdusting and saving the server config",ex);
            }

            try
            {
                if (serverUpdateTask != null && serverUpdateTask.IsCompleted)
                {
                    serverUpdateTask.Dispose();
                }
                else if (SteamCMDManager.CheckSteamCMD())
                {
                    SteamCMDManager.KillSteamCMD();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when killing steamCmd",ex);
            }
        }

        public static void KillServerOnClose()
        {
            kill = true;
            if (dayZServer != null)
            {
                dayZServer.KillServerProcess();

                try
                {
                    if (scheduler != null)
                    {
                        scheduler.KillAutomaticTasks();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error when killing server on close", ex);
                }
            }
            if (serverLoop != null)
            {
                serverLoop = null;
            }
        }

        public static string SetSteamGuard(string code)
        {
            try
            {
                if (dayZServer != null)
                {
                    return SteamCMDManager.WriteSteamGuard(code);
                }
                else
                {
                    return "DayZServer not running";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when setting SteamGuard",ex);
                return "Error";
            }
        }

        public static string GetAdminLog()
        {
            try
            {
                string returnString = string.Empty;
                string adminLogPath = Path.Combine(SERVER_PATH, managerConfig.profileName, ADMIN_LOG_NAME);
                if (!File.Exists(adminLogPath) && File.Exists(Path.Combine(SERVER_PATH, managerConfig.profileName, ADMIN_LOG_X64_NAME)))
                {
                    adminLogPath = Path.Combine(SERVER_PATH, managerConfig.profileName, ADMIN_LOG_X64_NAME);
                }

                using (var fs = new FileStream(adminLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.Default))
                    {
                        returnString = sr.ReadToEnd();
                    }
                }
                props.adminLog = returnString;
                return returnString;
            }
            catch (Exception ex)
            {
                Logger.Error("Error when getting admin log", ex);
                return string.Empty;
            }
        }

        public static bool CheckScheduler()
        {
            try
            {
                if (scheduler != null && !(dayZServer != null && dayZServer.RestartingForUpdates))
                {
                    if (!scheduler.IsConnected() && (connectTask == null || connectTask.IsCompleted))
                    {
                        scheduler.KillAutomaticTasks();
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (dayZServer != null && dayZServer.RestartingForUpdates)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when schecking scheduler", ex);
                return false;
            }
        }

        public static void StartScheduler()
        {
            try
            {
                bool onlyRestarts = managerConfig.clientMods.FindAll(mod => mod.name.ToLower().Contains(EXPANSION_MOD_SEARCH)).Count > 0;

                try
                {
                    string battleyeFolderPath = OperatingSystem.IsWindows() ? Path.Combine(SERVER_PATH, managerConfig.profileName, BATTLEYE_FOLDER_NAME) : Path.Combine(SERVER_PATH, BATTLEYE_FOLDER_NAME);

                    List<string> beFiles = FileSystem.GetFiles(battleyeFolderPath).ToList();
                    foreach (string beFile in beFiles)
                    {
                        if (Path.GetExtension(beFile) == ".cfg" && Path.GetFileNameWithoutExtension(beFile).Contains(Path.GetFileNameWithoutExtension(BATTLEYE_CONFIG_NAME)))
                        {
                            UpdateBeConfig(beFile);
                        }
                    }

                    Logger.Info(STATUS_SCHEDULER_UPDATED);
                    props.managerStatus = STATUS_SCHEDULER_UPDATED;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error when updating be file",ex);
                }

                scheduler = new SchedulerManager(LOCALHOST, managerConfig.RConPort, managerConfig.RConPassword, managerConfig.restartInterval, onlyRestarts, managerConfig.customMessages);
                connectTask = new Task(() => { scheduler.Connect(); });
                connectTask.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Error when starting scheduler",ex);
            }
        }

        public static void UpdateExpansionNotificationFile()
        {
            if (managerConfig.clientMods.FindAll(mod => mod.name.ToLower().Contains(EXPANSION_MOD_SEARCH)).Count > 0)
            {
                if (!Directory.Exists(Path.Combine(SERVER_PATH, managerConfig.profileName, PROFILE_EXPANSIONMOD_FOLDER_NAME)))
                {
                    Directory.CreateDirectory(Path.Combine(SERVER_PATH, managerConfig.profileName, PROFILE_EXPANSIONMOD_FOLDER_NAME));
                }

                if (!Directory.Exists(Path.Combine(SERVER_PATH, managerConfig.profileName, PROFILE_EXPANSIONMOD_FOLDER_NAME, PROFILE_EXPANSION_SETTINGS_FOLDER_NAME)))
                {
                    Directory.CreateDirectory(Path.Combine(SERVER_PATH, managerConfig.profileName, PROFILE_EXPANSIONMOD_FOLDER_NAME, PROFILE_EXPANSION_SETTINGS_FOLDER_NAME));
                }

                NotificationSchedulerFile? notFile = JSONSerializer.DeserializeJSONFile<NotificationSchedulerFile>(Path.Combine(SERVER_PATH, managerConfig.profileName, PROFILE_EXPANSIONMOD_FOLDER_NAME, PROFILE_EXPANSION_SETTINGS_FOLDER_NAME, PROFILE_EXPANSION_NOTIFICATION_SCHEDULER_SETTINGS_FILE_NAME));
                if (notFile == null)
                {
                    notFile = new NotificationSchedulerFile();
                }
                RestartUpdater.UpdateExpansionScheduler(managerConfig, notFile);
                JSONSerializer.SerializeJSONFile(Path.Combine(SERVER_PATH, managerConfig.profileName, PROFILE_EXPANSIONMOD_FOLDER_NAME, PROFILE_EXPANSION_SETTINGS_FOLDER_NAME, PROFILE_EXPANSION_NOTIFICATION_SCHEDULER_SETTINGS_FILE_NAME), notFile);
            }
        }

        private static void UpdateBeConfig(string path)
        {
            try
            {
                string beConfig = $"RConPassword {Manager.managerConfig.RConPassword}";
                beConfig += $"{Environment.NewLine}RConPort {Manager.managerConfig.RConPort}";
                beConfig += $"{Environment.NewLine}RestrictRCon 0";

                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(beConfig);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when updating be config", ex);
            }
        }

        #region ManagerConfig

        public static void PostManagerConfig(ManagerConfig config)
        {
            managerConfig = config;
            SaveManagerConfig();
        }

        public static void LoadManagerConfig()
        {
            if (File.Exists(MANAGER_CONFIG_NAME))
            {
                ManagerConfig? deserializedManagerConfig = JSONSerializer.DeserializeJSONFile<ManagerConfig>(MANAGER_CONFIG_NAME);

                if (deserializedManagerConfig != null)
                {
                    managerConfig = deserializedManagerConfig;
                }
            }
            else
            {
                managerConfig = new ManagerConfig();
                SaveManagerConfig();
            }
        }

        private static void SaveManagerConfig()
        {
            JSONSerializer.SerializeJSONFile<ManagerConfig>(MANAGER_CONFIG_NAME, managerConfig);
        }
        #endregion ManagerConfig
    }
}
