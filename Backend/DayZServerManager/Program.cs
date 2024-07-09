// See https://aka.ms/new-console-template for more information
using DayZServerManager;
using DayZServerManager.Helpers;
using DayZServerManager.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.SerializationClasses.BecClasses;
using DayZServerManager.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.SerializationClasses.ServerConfigClasses;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

string configName = "Config.json";

if (FileSystem.FileExists(configName))
{
    Config? config = readConfig(configName);

    if (config != null)
    {
        StartServer(config);
    }
}
else
{
    Config config = new Config();
    SetStandardConfig(config);
    SaveConfig(config);
    StartServer(config);
}

void StartServer(Config config)
{

    if (string.IsNullOrEmpty(config.steamUsername) || string.IsNullOrEmpty(config.steamPassword))
    {
        return;
    }

    Server dayZServer = new Server(config);
    BackupAndUpdate(dayZServer);

    UpdateServerConfig(config);

    AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

    UpdateBECScheduler(config);

    dayZServer.StartServer();
    dayZServer.StartBEC();
    Thread.Sleep(30000);

    int i = 20;
    bool kill = false;
    while (!kill)
    {
        if (!dayZServer.CheckServer())
        {
            dayZServer.BackupServerData();
            dayZServer.UpdateServer();
            dayZServer.UpdateAndMoveMods(false, true);
            dayZServer.StartServer();
        }
        else
        {
            WriteToConsole("The Server is still running");
        }
        if (!dayZServer.CheckBEC())
        {
            dayZServer.StartBEC();
        }
        else
        {
            WriteToConsole("BEC is still running");
        }
        if (i % 4 == 0)
        {
            dayZServer.UpdateAndMoveMods(true, false);
            dayZServer.CheckForUpdatedMods();
        }
        i++;
        Thread.Sleep(30000);
    }
    dayZServer.KillServerProcesses();

    void OnProcessExit(object? sender, EventArgs? e)
    {
        dayZServer.KillServerProcesses();
    }
}

Config? readConfig(string name)
{
    try
    {
        using (StreamReader reader = new StreamReader(name))
        {
            string json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<Config>(json);
        }
    }
    catch (Exception ex)
    {
        WriteToConsole(ex.ToString());
        return null;
    }
}

void SaveConfig(Config config)
{
    try
    {
        using (StreamWriter writer = new StreamWriter("Config.json"))
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string json = JsonSerializer.Serialize(config, options);
            writer.Write(json);
            writer.Close();
        }
    }
    catch (Exception ex)
    {
        WriteToConsole(ex.ToString());
    }
}

void BackupAndUpdate(Server server)
{
    server.BackupServerData();
    server.UpdateServer();
    server.UpdateBEC();
    server.UpdateAndMoveMods(true, true);
}

void UpdateServerConfig(Config config)
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

void UpdateBECScheduler(Config config)
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

                RestartUpdater.UpdateRestartScripts(config.RestartInterval, becScheduler, expansionScheduler);

                SerializeNotificationSchedulerFile(Path.Combine(config.serverPath, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"), expansionScheduler);
            }
            else
            {
                RestartUpdater.UpdateRestartScripts(config.RestartInterval, becScheduler);
            }

            SerializeSchedulerFile(Path.Combine(config.becPath, "Config", "Scheduler.xml"), becScheduler);
        }
    }
    catch (Exception ex)
    {
        WriteToConsole(ex.ToString());
    }
}


void WriteToConsole(string message)
{
    Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
}

void AdjustServerConfig(Config config, ServerConfig serverConfig)
{
    serverConfig.template = config.missionName;
    serverConfig.instanceId = config.instanceId;
    serverConfig.steamQueryPort = config.steamQueryPort;
}

void SetStandardConfig(Config config)
{
    config.steamUsername = StandardManagerConfig.STEAMUSERNAME;
    config.steamPassword = StandardManagerConfig.STEAMPASSWORD;
    config.serverPath = StandardManagerConfig.SERVERPATH;
    config.steamCMDPath = StandardManagerConfig.STEAMCMDPATH;
    config.becPath = StandardManagerConfig.BECPATH;
    config.workshopPath = StandardManagerConfig.WORKSHOPPATH;
    config.backupPath = StandardManagerConfig.BACKUPPATH;
    config.missionName = StandardManagerConfig.MISSIONNAME;
    config.instanceId = StandardManagerConfig.INSTANCEID;
    config.serverConfigName = StandardManagerConfig.SERVERCONFIGNAME;
    config.profileName = StandardManagerConfig.PROFILENAME;
    config.port = StandardManagerConfig.PORT;
    config.steamQueryPort = StandardManagerConfig.STEAMQUERYPORT;
    config.RConPort = StandardManagerConfig.RCONPORT;
    config.cpuCount = StandardManagerConfig.CPUCOUNT;
    config.noFilePatching = StandardManagerConfig.NOFILEPATCHING;
    config.doLogs = StandardManagerConfig.DOLOGS;
    config.adminLog = StandardManagerConfig.ADMINLOG;
    config.netLog = StandardManagerConfig.NETLOG;
    config.freezeCheck = StandardManagerConfig.FREEZECHECK;
    config.limitFPS = StandardManagerConfig.LIMITFPS;
    config.vanillaMissionName = StandardManagerConfig.VANILLAMISSIONNAME;
    config.missionTemplatePath = StandardManagerConfig.MISSIONTEMPLATEPATH;
    config.expansionDownloadPath = StandardManagerConfig.EXPANSIONDOWNLOADPATH;
    config.mapName = StandardManagerConfig.MAPNAME;
    config.RestartOnUpdate = StandardManagerConfig.RESTARTONUPDATE;
    config.RestartInterval = StandardManagerConfig.RESTARTINTERVAL;
    config.clientMods = new List<Mod>();
    config.serverMods = new List<Mod>();
    Mod mod1 = new Mod();
    mod1.workshopID = 1559212036;
    mod1.name = "@CF";
    config.clientMods.Add(mod1);
    Mod mod2 = new Mod();
    mod2.workshopID = 1564026768;
    mod2.name = "@Community-Online-Tools";
    config.clientMods.Add(mod2);
}

#region SchedulerFile
void SerializeSchedulerFile(string path, SchedulerFile schedulerFile)
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
SchedulerFile? DeserializeSchedulerFile(string path)
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
void SerializeNotificationSchedulerFile(string path, NotificationSchedulerFile schedulerFile)
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
NotificationSchedulerFile? DeserializeNotificationSchedulerFile(string path)
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