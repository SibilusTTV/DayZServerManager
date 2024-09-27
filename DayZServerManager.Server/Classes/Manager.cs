using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using System.Text.Json;

namespace DayZServerManager.Server.Classes
{
    internal static class Manager
    {
        public static ManagerConfig? managerConfig;
        public static ServerConfig? serverConfig;
        public static Server? dayZServer;
        public static Task? serverLoop;
        public static ManagerProps? props;
        public static bool kill = false;

        #region Constants
        public const string MANAGER_CONFIG_NAME = "config.json";
        public const string SERVER_PATH = "server";
        public const string SERVER_DEPLOY = "deploy";
        public static string SERVER_EXECUTABLE = OperatingSystem.IsWindows() ? "DayZServer_x64.exe" : "DayZServer";
        public const string STEAM_CMD_PATH = "steamcmd";
        public static string STEAM_CMD_EXECUTABLE = OperatingSystem.IsWindows() ? "steamcmd.exe" : "steamcmd.sh";
        public static string STEAM_CMD_ZIP_NAME = OperatingSystem.IsWindows() ? "steamcmd.zip" : "steamcmd.tar.gz";
        public const string STEAMCMD_DOWNLOAD_URL = "https://steamcdn-a.akamaihd.net/client/installer/";
        public const string STEAMCMD_TAR_FILE_NAME = "steamcmd.tar";
        public const string MODS_PATH = "mods";
        public static string WORKSHOP_PATH = Path.Combine("steamapps", "workshop", "content", "221100");
        public static string MPMISSIONS_PATH = Path.Combine(SERVER_PATH, "mpmissions");
        public static string EXPANSION_DOWNLOAD_PATH = Path.Combine(MPMISSIONS_PATH, "DayZ-Expansion-Missions");
        public static string BATTLEYE_FOLDER_NAME = OperatingSystem.IsWindows() ? "BattlEye" : "battleye";
        public static string BATTLEYE_CONFIG_NAME = OperatingSystem.IsWindows() ? "BEServer_x64.cfg" : "beserver_x64.cfg";
        public const string BATTLEYE_BANS_NAME = "Bans.txt";
        public const string SCHEDULER_DOWNLOAD_URL = "https://github.com/SibilusTTV/DayZScheduler/releases/latest/download/";
        public static string SCHEDULER_ZIP_NAME = OperatingSystem.IsWindows() ? "windows.zip" : "linux.zip";
        public const string SCHEDULER_PATH = "scheduler";
        public const string SCHEDULER_CONFIG_FOLDER = "config";
        public const string SCHEDULER_CONFIG_NAME = "config.json";
        public const string SCHEDULER_CONFIG_UPDATE_NAME = "config-update.json";
        public const string SCHEDULER_FILE_NAME = "scheduler.json";
        public const string SCHEDULER_FILE_UPDATE_NAME = "scheduler-update.json";
        public static string SCHEDULER_EXECUTABLE = OperatingSystem.IsWindows() ? "DayZScheduler.exe" : "DayZScheduler";
        public const int DAYZ_SERVER_BRANCH = 223350;
        public const int DAYZ_GAME_BRANCH = 221100;

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
        #endregion Constants

        public static void InitiateManager()
        {
            LoadManagerConfig();
            props = new ManagerProps(STATUS_LISTENING, STATUS_NOT_RUNNING, STATUS_NOT_RUNNING, 0);

            if (managerConfig != null)
            {
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

                LoadServerConfig();
                AdjustServerConfig();
                SaveManagerConfig();
                SaveServerConfig();

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
        }

        public static void StartServer()
        {
            if (props != null && managerConfig != null)
            {
                if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
                {
                    props.managerStatus = STATUS_CREDENTIALS;
                    return;
                }

                dayZServer ??= new Server();

                if (serverConfig != null)
                {
                    SaveServerConfig();
                }

                UpdateAndBackupServer(dayZServer, true, true);

                kill = false;
                props.managerStatus = STATUS_STARTING_SERVER;
                dayZServer.StartServer();

                props.managerStatus = STATUS_STARTING_SCHEDULER;
                dayZServer.StartScheduler();

                props.managerStatus = STATUS_LISTENING;
                props.dayzServerStatus = STATUS_STARTED;

                serverLoop = new Task(() => { StartServerLoop(); });
                serverLoop.Start();
            }
        }

        private static void StartServerLoop()
        {
            if (props != null)
            {

                Thread.Sleep(10000);

                double i = 10;
                while (!kill)
                {
                    if (dayZServer != null)
                    {
                        if (!dayZServer.CheckServer())
                        {
                            Thread.Sleep(10000);
                            i += 10;
                            SaveServerConfig();

                            UpdateAndBackupServer(dayZServer, false, true);

                            dayZServer.StartServer();
                        }
                        else
                        {
                            WriteToConsole("The Server is still running");
                        }

                        if (!dayZServer.CheckScheduler())
                        {
                            dayZServer.StartScheduler();
                        }
                        else
                        {
                            WriteToConsole("Scheduler is still running");
                        }

                        if (i % 120 == 0)
                        {
                            UpdateAndBackupServer(dayZServer, true, false);

                            dayZServer.RestartForUpdates();
                        }
                        i += 10;
                        props.managerStatus = STATUS_LISTENING;
                        Thread.Sleep(10000);
                    }
                }

                WriteToConsole(STATUS_STOPPING_SERVER);

                KillServerProcesses();

                Thread.Sleep(10000);

                SaveServerConfig();

                props.managerStatus = STATUS_LISTENING;

                WriteToConsole("Server was stopped");
            }
        }

        public static void StopServer()
        {
            if (props != null)
            {
                if (dayZServer != null && serverLoop != null)
                {
                    kill = true;
                    props.managerStatus = STATUS_STOPPING_SERVER;
                }
                else
                {
                    KillServerProcesses();
                }
            }
        }

        public static void KillServerProcesses()
        {
            if (dayZServer != null)
            {
                dayZServer.KillServerProcesses();
                Thread.Sleep(10000);
                dayZServer = null;

                if (serverConfig != null)
                {
                    SaveServerConfig();
                }
            }
        }

        public static void KillServerOnClose()
        {
            kill = true;
            if (dayZServer != null)
            {
                dayZServer.KillServerProcesses();
            }
            if (serverLoop != null)
            {
                serverLoop = null;
            }
        }

        private static void UpdateAndBackupServer(Server server, bool hasToUpdate, bool hasToMove)
        {
            if (props != null)
            {
                if (hasToMove)
                {
                    server.UpdateScheduler();

                    if (managerConfig != null && managerConfig.makeBackups)
                    {
                        server.BackupServerData(props);
                    }
                }

                server.UpdateAndMoveServer(props, hasToUpdate, hasToMove);
                
                server.UpdateAndMoveMods(props, hasToUpdate, hasToMove);
            }
        }

        public static string SetSteamGuard(string code)
        {
            try
            {
                if (dayZServer != null)
                {
                    return dayZServer.WriteSteamGuard(code);
                }
                else
                {
                    return "DayZServer not running";
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                return "Error";
            }
        }

        #region ServerConfig
        public static string GetServerConfig()
        {
            if (serverConfig != null)
            {
                return ServerConfigSerializer.Serialize(serverConfig);
            }
            else
            {
                return "";
            }
        }

        public static void PostServerConfig(string newServerConfig)
        {
            serverConfig = ServerConfigSerializer.Deserialize(newServerConfig);
        }

        public static void LoadServerConfig()
        {
            if (managerConfig != null && File.Exists(Path.Combine(SERVER_PATH, managerConfig.serverConfigName)))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(SERVER_PATH, managerConfig.serverConfigName)))
                    {
                        string serverConfigText = reader.ReadToEnd();
                        serverConfig = ServerConfigSerializer.Deserialize(serverConfigText);
                    }
                }
                catch (Exception ex)
                {
                    WriteToConsole(ex.ToString());
                }
            }
            else
            {
                try
                {
                    if (!Directory.Exists(SERVER_PATH))
                    {
                        Directory.CreateDirectory(SERVER_PATH);
                    }
                    serverConfig = new ServerConfig();
                }
                catch (Exception ex)
                {
                    WriteToConsole(ex.ToString());
                }
            }
        }

        public static void AdjustServerConfig()
        {
            if (serverConfig != null && managerConfig != null)
            {
                serverConfig.template = managerConfig.missionName;
                serverConfig.instanceId = managerConfig.instanceId;
                serverConfig.steamQueryPort = managerConfig.steamQueryPort;
            }
        }

        public static void SaveServerConfig()
        {
            try
            {
                if (serverConfig != null && managerConfig != null)
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(SERVER_PATH, managerConfig.serverConfigName)))
                    {
                        string serverConfigText = ServerConfigSerializer.Serialize(serverConfig);
                        writer.Write(serverConfigText);
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }
        #endregion ServerConfig

        #region ManagerConfig
        public static string GetManagerConfig()
        {
            if (Manager.managerConfig != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                return JsonSerializer.Serialize(Manager.managerConfig, options);
            }
            else
            {
                return "";
            }
        }

        public static void PostManagerConfig(string newConfig)
        {
            ManagerConfig? newConfigObject = JsonSerializer.Deserialize<ManagerConfig>(newConfig);
            if (newConfigObject != null)
            {
                managerConfig = newConfigObject;
            }
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

        public static void SaveManagerConfig()
        {
            if (managerConfig != null)
            {
                JSONSerializer.SerializeJSONFile<ManagerConfig>(MANAGER_CONFIG_NAME, managerConfig);
            }
        }
        #endregion ManagerConfig

        public static void WriteToConsole(string message)
        {
            System.Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
        }
    }
}
