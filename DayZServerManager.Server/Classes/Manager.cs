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

namespace DayZServerManager.Server.Classes
{
    internal static class Manager
    {
        public static ManagerConfig? managerConfig;
        public static ServerConfig? serverConfig;
        public static Server? dayZServer;
        public static Task? serverLoop;
        public static ManagerProps? props;
        public const string MANAGERCONFIGNAME = "Config.json";

        public static void StartServer()
        {
            if (props != null && managerConfig != null)
            {
                dayZServer = new Server(managerConfig);

                if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
                {
                    props._serverStatus = "Please set Username and Password";
                }

                BackupAndUpdate(dayZServer);

                UpdateBECScheduler(managerConfig);

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
                props = new ManagerProps("No Manager Config", "");
            }
        }

        private static void StartServerLoop()
        {
            if (props != null)
            {

                Thread.Sleep(60000);

                int i = 20;
                while (!dayZServer.Kill)
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
                dayZServer.KillServerProcesses();

                SaveServerConfig();

                props._serverStatus = "Server stopped";
            }
        }

        public static void KillServerProcesses()
        {
            if (dayZServer != null)
            {
                dayZServer.KillServerProcesses();
            }
        }

        private static void BackupAndUpdate(Server server)
        {
            if (props != null)
            {
                props._serverStatus = "Backing up server";
                server.BackupServerData();
                props._serverStatus = "Updating server";
                server.UpdateServer(props);
                props._serverStatus = "Updating BEC";
                server.UpdateBEC();
                props._serverStatus = "Updating mods";
                server.UpdateAndMoveMods(props, true, true);
                props._serverStatus = "Backed up and updated";
            }
        }

        private static void UpdateBECScheduler(ManagerConfig config)
        {
            try
            {
                if (FileSystem.FileExists(Path.Combine(config.becPath, "Config", "Scheduler.xml")))
                {
                    SchedulerFile? becScheduler = XMLSerializer.DeserializeXMLFile<SchedulerFile>(Path.Combine(config.becPath, "Config", "Scheduler.xml"));
                    if (becScheduler == null)
                    {
                        becScheduler = new SchedulerFile();
                    }

                    // Checking if one of the clientMods has expansion in its name
                    if (config.clientMods != null && (config.clientMods.FindAll(x => x.name.Contains("expansion") || x.name.Contains("Expansion")).Count > 0))
                    {
                        NotificationSchedulerFile? expansionScheduler;
                        if (FileSystem.FileExists(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json")))
                        {
                            expansionScheduler = JSONSerializer.DeserializeJSONFile<NotificationSchedulerFile>(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"));
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

                        JSONSerializer.SerializeJSONFile<NotificationSchedulerFile>(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"), expansionScheduler);
                    }
                    else
                    {
                        RestartUpdater.UpdateRestartScripts(config.restartInterval, becScheduler);
                    }

                    XMLSerializer.SerializeXMLFile<SchedulerFile>(Path.Combine(config.becPath, "Config", "Scheduler.xml"), becScheduler);
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
            if (FileSystem.FileExists(Path.Combine(managerConfig.serverPath, managerConfig.serverConfigName)))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(managerConfig.serverPath, managerConfig.serverConfigName)))
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
                    if (!FileSystem.DirectoryExists(managerConfig.serverPath))
                    {
                        FileSystem.CreateDirectory(managerConfig.serverPath);
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
            serverConfig.template = managerConfig.missionName;
            serverConfig.instanceId = managerConfig.instanceId;
            serverConfig.steamQueryPort = managerConfig.steamQueryPort;
        }

        public static void SaveServerConfig()
        {
            if (serverConfig != null)
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(managerConfig.serverPath, managerConfig.serverConfigName)))
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
                ManagerConfig? deserializedManagerConfig = JSONSerializer.DeserializeJSONFile<ManagerConfig>(MANAGERCONFIGNAME);

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
                JSONSerializer.SerializeJSONFile<ManagerConfig>(MANAGERCONFIGNAME, managerConfig);
            }
        }
        #endregion ManagerConfig

        public static void WriteToConsole(string message)
        {
            System.Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
        }
    }
}
