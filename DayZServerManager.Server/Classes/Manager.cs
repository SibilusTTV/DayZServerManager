using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
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
        public const string MANAGER_CONFIG_NAME = "Config.json";
        public const string SERVER_PATH = "Server";
        public const string STEAM_CMD_PATH = "SteamCMD";
        public const string BEC_PATH = "BEC";
        public const string MODS_PATH = "Mods";
        public static string WORKSHOP_PATH = Path.Combine("steamapps", "workshop", "content", "221100");
        public const string PROFILE_NAME = "Profiles";
        public static string MISSION_PATH = Path.Combine(SERVER_PATH, "mpmissions");
        public static string EXPANSION_DOWNLOAD_PATH = Path.Combine(MISSION_PATH, "DayZ-Expansion-Missions");
        public static string STEAM_CMD_EXECUTABLE = OperatingSystem.IsWindows() ? "steamcmd.exe" : "steamcmd.sh";
        public static string STEAM_CMD_ZIP_NAME = OperatingSystem.IsWindows() ? "steamcmd.zip" : "steamcmd.tar.gz";
        public static string BATTLEYE_FOLDER_NAME = OperatingSystem.IsWindows() ? "BattlEye" : "battleye";
        public static string BATTLEYE_CONFIG_NAME = OperatingSystem.IsWindows() ? "BEServer_x64.cfg" : "beserver_x64.cfg";
        public const string BEC_EXECUTABLE = "Bec.exe";
        public static string SERVER_EXECUTABLE = OperatingSystem.IsWindows() ? "DayZServer_x64.exe" : "DayZServer";
        public const string BATTLEYE_BANS_NAME = "Bans.txt";
        #endregion Constants

        public static void InitiateManager()
        {
            Manager.LoadManagerConfig();

            if (managerConfig != null)
            {
                props = new ManagerProps("Waiting for Starting");

                if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
                {
                    props._serverStatus = "Please set Username and Password";
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
                if (!directories.Contains(BEC_PATH))
                {
                    FileSystem.CreateDirectory(BEC_PATH);
                }

                UpdateBECScheduler(managerConfig);

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
                    props._serverStatus = "Please set Username and Password";
                    return;
                }

                if (dayZServer == null)
                {
                    dayZServer = new Server(managerConfig);
                }
                else
                {
                    props._serverStatus = "Server already running";
                    return;
                }

                if (serverConfig != null)
                {
                    SaveServerConfig();
                }

                BackupServer(dayZServer);
                UpdateServer(dayZServer);

                kill = false;
                dayZServer.StartServer();

                if (OperatingSystem.IsWindows())
                {
                    dayZServer.StartBEC();
                }

                props._serverStatus = "Server started";

                serverLoop = new Task(() => { StartServerLoop(); });
                serverLoop.Start();
            }
            else
            {
                props = new ManagerProps("No Manager Config");
            }
        }

        private static void StartServerLoop()
        {
            if (props != null)
            {

                Thread.Sleep(60000);

                int i = 20;
                while (!kill)
                {
                    if (dayZServer != null)
                    {
                        if (!dayZServer.CheckServer())
                        {
                            dayZServer.BackupServerData();
                            dayZServer.UpdateServer(props);
                            dayZServer.UpdateAndMoveMods(props, false, true);
                            dayZServer.StartServer();
                        }
                        else
                        {
                            WriteToConsole("The Server is still running");
                        }

                        if (!dayZServer.CheckBEC())
                        {
                            if (OperatingSystem.IsWindows())
                            {
                                dayZServer.StartBEC();
                            }
                        }
                        else
                        {
                            WriteToConsole("BEC is still running");
                        }

                        if (i % 4 == 0)
                        {
                            dayZServer.UpdateAndMoveMods(props, true, false);
                            dayZServer.CheckForUpdatedMods();
                        }
                        i++;
                        Thread.Sleep(60000);
                    }
                }

                WriteToConsole("Stopping Server");

                KillServerProcesses();

                Thread.Sleep(10000);

                SaveServerConfig();

                props._serverStatus = "Server stopped";

                WriteToConsole("Server was stopped");
            }
        }

        public static void StopServer()
        {
            kill = true;
        }

        public static void KillServerProcesses()
        {
            if (dayZServer != null)
            {
                dayZServer.KillServerProcesses();
                dayZServer = null;
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

        private static void UpdateServer(Server server)
        {
            if (props != null)
            {
                props._serverStatus = "Updating server";
                server.UpdateServer(props);
                props._serverStatus = "Updating BEC";
                server.UpdateBEC();
                props._serverStatus = "Updating mods";
                server.UpdateAndMoveMods(props, true, true);
                props._serverStatus = "Server, Mods and BEC updated";
            }
        }

        private static void BackupServer(Server server)
        {
            if (props != null)
            {
                props._serverStatus = "Backing up server";
                server.BackupServerData();
                props._serverStatus = "Backed up server";
            }

        }

        private static void UpdateBECScheduler(ManagerConfig config)
        {
            try
            {
                if (FileSystem.FileExists(Path.Combine(BEC_PATH, "Config", "Scheduler.xml")))
                {
                    SchedulerFile? becScheduler = XMLSerializer.DeserializeXMLFile<SchedulerFile>(Path.Combine(BEC_PATH, "Config", "Scheduler.xml"));
                    if (becScheduler == null)
                    {
                        becScheduler = new SchedulerFile();
                    }

                    // Checking if one of the clientMods has expansion in its name
                    if (config.clientMods != null && (config.clientMods.FindAll(x => x.name.ToLower().Contains("expansion")).Count > 0))
                    {
                        NotificationSchedulerFile? expansionScheduler;
                        if (FileSystem.FileExists(Path.Combine(SERVER_PATH, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json")))
                        {
                            expansionScheduler = JSONSerializer.DeserializeJSONFile<NotificationSchedulerFile>(Path.Combine(SERVER_PATH, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"));
                            if (expansionScheduler == null)
                            {
                                expansionScheduler = new NotificationSchedulerFile();
                            }
                        }
                        else
                        {
                            expansionScheduler = new NotificationSchedulerFile();
                        }

                        RestartUpdater.UpdateRestartScripts(config.restartInterval, becScheduler, expansionScheduler);

                        JSONSerializer.SerializeJSONFile<NotificationSchedulerFile>(Path.Combine(SERVER_PATH, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"), expansionScheduler);
                    }
                    else
                    {
                        RestartUpdater.UpdateRestartScripts(config.restartInterval, becScheduler);
                    }

                    XMLSerializer.SerializeXMLFile<SchedulerFile>(Path.Combine(BEC_PATH, "Config", "Scheduler.xml"), becScheduler);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
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
            string configName = "Config.json";

            if (FileSystem.FileExists(configName))
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
