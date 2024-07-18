using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;

namespace DayZServerManager.Server.Classes
{
    internal static class Manager
    {
        public static ManagerConfig managerConfig;
        public static ServerConfig serverConfig;
        public static Server server;
        public const string MANAGERCONFIGNAME = "Config.json";

        public static string StartServer()
        {
            if (managerConfig != null)
            {
                server = new Server(managerConfig);

                if (string.IsNullOrEmpty(managerConfig.steamUsername) || string.IsNullOrEmpty(managerConfig.steamPassword))
                {
                    return "Please set Username and Password";
                }

                BackupAndUpdate(server);

                UpdateBECScheduler(managerConfig);

                //dayZServer.StartServer();
                //dayZServer.StartBEC();
                //Thread.Sleep(30000);

                //int i = 20;
                //bool kill = false;
                //while (!kill)
                //{
                //    if (!dayZServer.CheckServer())
                //    {
                //        dayZServer.BackupServerData();
                //        dayZServer.UpdateServer();
                //        dayZServer.UpdateAndMoveMods(false, true);
                //        dayZServer.StartServer();
                //    }
                //    else
                //    {
                //        WriteToConsole("The Server is still running");
                //    }
                //    if (!dayZServer.CheckBEC())
                //    {
                //        dayZServer.StartBEC();
                //    }
                //    else
                //    {
                //        WriteToConsole("BEC is still running");
                //    }
                //    if (i % 4 == 0)
                //    {
                //        dayZServer.UpdateAndMoveMods(true, false);
                //        dayZServer.CheckForUpdatedMods();
                //    }
                //    i++;
                //    Thread.Sleep(30000);
                //}
                //server.KillServerProcesses();

                SaveServerConfig();

                return "Server was started";
            }
            else
            {
                return "No Manager Config";
            }
        }
    
        public static void KillServerProcesses()
        {
            if (server != null)
            {
                server.KillServerProcesses();
            }
        }

        private static void BackupAndUpdate(Server server)
        {
            server.BackupServerData();
            server.UpdateServer();
            server.UpdateBEC();
            server.UpdateAndMoveMods(true, true);
        }

        private static void UpdateBECScheduler(ManagerConfig config)
        {
            try
            {
                if (FileSystem.FileExists(Path.Combine(config.becPath, "Config", "Scheduler.xml")))
                {
                    SchedulerFile? becScheduler = SchedulerFileSerializer.DeserializeSchedulerFile(Path.Combine(config.becPath, "Config", "Scheduler.xml"));
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
                            expansionScheduler = NotificationSchedulerFileSerializer.DeserializeNotificationSchedulerFile(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"));
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

                        NotificationSchedulerFileSerializer.SerializeNotificationSchedulerFile(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"), expansionScheduler);
                    }
                    else
                    {
                        RestartUpdater.UpdateRestartScripts(config.restartInterval, becScheduler);
                    }

                    SchedulerFileSerializer.SerializeSchedulerFile(Path.Combine(config.becPath, "Config", "Scheduler.xml"), becScheduler);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
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
                ManagerConfig? deserializedManagerConfig = ManagerConfigSerializer.DeserializeManagerConfig();

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
                ManagerConfigSerializer.SerializeManagerConfig(managerConfig);
            }
        }
        #endregion ManagerConfig

        public static void WriteToConsole(string message)
        {
            System.Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
        }
    }
}
