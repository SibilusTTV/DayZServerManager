using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting.Server;

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
        public const string MODS_PATH = "mods";
        public static string WORKSHOP_PATH = Path.Combine("steamapps", "workshop", "content", "221100");
        public const string PROFILE_NAME = "Profiles";
        public static string MISSION_PATH = Path.Combine(SERVER_PATH, "mpmissions");
        public static string EXPANSION_DOWNLOAD_PATH = Path.Combine(MISSION_PATH, "DayZ-Expansion-Missions");
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
        #endregion Constants

        public static void InitiateManager()
        {
            LoadManagerConfig();
            props = new ManagerProps("Listening", "Not Running", "Not Running", 0);

            if (managerConfig != null)
            {
                if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
                {
                    props.managerStatus = "Credentials";
                    return;
                }

                List<string> directories = FileSystem.GetDirectories(Environment.CurrentDirectory).ToList<string>();
                if (!directories.Contains(SERVER_PATH))
                {
                    FileSystem.CreateDirectory(SERVER_PATH);
                }
                if (!directories.Contains(STEAM_CMD_PATH))
                {
                    FileSystem.CreateDirectory(STEAM_CMD_PATH);
                }
                if (!directories.Contains(MODS_PATH))
                {
                    FileSystem.CreateDirectory(MODS_PATH);
                }
                if (!directories.Contains(SCHEDULER_PATH))
                {
                    FileSystem.CreateDirectory(SCHEDULER_PATH);
                }

                LoadServerConfig();
                AdjustServerConfig();
                SaveManagerConfig();
                SaveServerConfig();

                if (managerConfig.autoStartServer)
                {
                    StartServer();
                }
            }
        }

        public static void StartServer()
        {
            if (props != null && managerConfig != null)
            {
                if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
                {
                    props.managerStatus = "Credentials";
                    return;
                }

                dayZServer ??= new Server();

                if (serverConfig != null)
                {
                    SaveServerConfig();
                }

                UpdateAndBackupServer(dayZServer, true, true);

                kill = false;
                props.managerStatus = "Starting Server";
                dayZServer.StartServer();

                props.managerStatus = "Starting Scheduler";
                dayZServer.StartScheduler();

                props.managerStatus = "Listening";
                props.dayzServerStatus = "Started";

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
                        props.managerStatus = "Listening";
                        Thread.Sleep(10000);
                    }
                }

                WriteToConsole("Stopping Server");

                KillServerProcesses();

                Thread.Sleep(10000);

                SaveServerConfig();

                props.managerStatus = "Listening";

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
                    props.managerStatus = "Stopping Server";
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
            if (managerConfig != null && FileSystem.FileExists(Path.Combine(SERVER_PATH, managerConfig.serverConfigName)))
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
                    if (!FileSystem.DirectoryExists(SERVER_PATH))
                    {
                        FileSystem.CreateDirectory(SERVER_PATH);
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
            if (FileSystem.FileExists(MANAGER_CONFIG_NAME))
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
