using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;

namespace DayZServerManager.Server.Classes
{
    internal static class Manager
    {
        public static Config config;
        public static Server server;

        public static void StartServer()
        {
            if (config != null && server != null)
            {

                if (string.IsNullOrEmpty(config.steamUsername) || string.IsNullOrEmpty(config.steamPassword))
                {
                    return;
                }

                BackupAndUpdate(server);

                UpdateServerConfig(config);

                UpdateBECScheduler(config);

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

        private static void UpdateServerConfig(Config config)
        {
            ServerConfig serverConfig;
            if (FileSystem.FileExists(Path.Combine(config.serverPath, config.serverConfigName)))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(config.serverPath, config.serverConfigName)))
                    {
                        string serverConfigText = reader.ReadToEnd();
                        serverConfig = ServerConfigSerializer.Deserialize(serverConfigText);
                    }
                    AdjustServerConfig(config, serverConfig);
                    using (StreamWriter writer = new StreamWriter(Path.Combine(config.serverPath, config.serverConfigName)))
                    {
                        string serverConfigText = ServerConfigSerializer.Serialize(serverConfig);
                        writer.Write(serverConfigText);
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
                    if (!FileSystem.DirectoryExists(config.serverPath))
                    {
                        FileSystem.CreateDirectory(config.serverPath);
                    }
                    serverConfig = new ServerConfig();
                    AdjustServerConfig(config, serverConfig);
                    using (StreamWriter writer = new StreamWriter(Path.Combine(config.serverPath, config.serverConfigName)))
                    {
                        string serverConfigText = ServerConfigSerializer.Serialize(serverConfig);
                        writer.Write(serverConfigText);
                        writer.Close();
                    }
                }
                catch (Exception ex)
                {
                    WriteToConsole(ex.ToString());
                }
            }
        }

        private static void AdjustServerConfig(Config config, ServerConfig serverConfig)
        {
            serverConfig.template = config.missionName;
            serverConfig.instanceId = config.instanceId;
            serverConfig.steamQueryPort = config.steamQueryPort;
        }

        private static void UpdateBECScheduler(Config config)
        {
            try
            {
                if (FileSystem.FileExists(Path.Combine(config.becPath, "Config", "Scheduler.xml")))
                {
                    SchedulerFile? becScheduler = DeserializeSchedulerFile(Path.Combine(config.becPath, "Config", "Scheduler.xml"));
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
                            expansionScheduler = DeserializeNotificationSchedulerFile(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"));
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

                        SerializeNotificationSchedulerFile(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"), expansionScheduler);
                    }
                    else
                    {
                        RestartUpdater.UpdateRestartScripts(config.restartInterval, becScheduler);
                    }

                    SerializeSchedulerFile(Path.Combine(config.becPath, "Config", "Scheduler.xml"), becScheduler);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        #region SchedulerFile
        private static void SerializeSchedulerFile(string path, SchedulerFile schedulerFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SchedulerFile));
                    serializer.Serialize(writer, schedulerFile);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }


        // Takes a path and returns the deserialized TypesFile
        private static SchedulerFile? DeserializeSchedulerFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(SchedulerFile));
                        return (SchedulerFile?)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                return null;
            }
        }
        #endregion SchedulerFile

        #region NotificationScheduler
        private static void SerializeNotificationSchedulerFile(string path, NotificationSchedulerFile schedulerFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(schedulerFile, options);
                    writer.Write(json);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }


        // Takes a path and returns the deserialized RarityFile
        private static NotificationSchedulerFile? DeserializeNotificationSchedulerFile(string path)
        {
            try
            {
                if (FileSystem.FileExists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<NotificationSchedulerFile>(json);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                return null;
            }
        }
        #endregion NotificationScheduler

        public static void WriteToConsole(string message)
        {
            Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
        }
    }
}
