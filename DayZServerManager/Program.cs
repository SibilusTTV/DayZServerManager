// See https://aka.ms/new-console-template for more information
using DayZServerManager;
using DayZServerManager.Helpers;
using DayZServerManager.ManagerConfigClasses;
using DayZServerManager.ServerConfigClasses;
using Microsoft.VisualBasic.FileIO;
using System.Text.Json;

if (FileSystem.FileExists("Config.json"))
{
    Config? config = null;
    try
    {
        using (StreamReader reader = new StreamReader("Config.json"))
        {
            string json = reader.ReadToEnd();
            config = JsonSerializer.Deserialize<Config>(json);
            reader.Close();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
    }

    if (config != null)
    {
        StartServer(config);
    }
}
else
{
    Config config = new Config();
    config.steamUsername = "";
    config.steamPassword = "";
    config.serverPath = "Server";
    config.steamCMDPath = "SteamCMD";
    config.becPath = "BEC";
    config.workshopPath = "SteamCMD\\steamapps\\workshop\\content\\221100";
    config.backupPath = "Backup";
    config.missionName = "Expansion.ChernarusPlus";
    config.instanceId = 1;
    config.serverConfigName = "serverDZ.cfg";
    config.profileName = "Profiles";
    config.port = 2302;
    config.steamQueryPort = 2305;
    config.RConPort = 2306;
    config.cpuCount = 8;
    config.noFilePatching = true;
    config.doLogs = true;
    config.adminLog = true;
    config.netLog = true;
    config.freezeCheck = true;
    config.limitFPS = -1;
    config.vanillaMissionName = "dayzOffline.chernarusplus";
    config.missionTemplatePath = Path.Combine(config.serverPath, "mpmissions", "ChernarusTemplate");
    config.expansionDownloadPath = Path.Combine(config.serverPath, "mpmissions", "DayZ-Expansion-Missions");
    config.mapName = "Chernarus";
    config.RestartOnUpdate = true;
    config.RestartInterval = 4;
    config.clientMods = new List<Mod>();
    config.serverMods = new List<Mod>();

    UpdateConfig(config);

    if (config != null)
    {
        StartServer(config);
    }
}

void StartServer(Config config)
{
    if (string.IsNullOrEmpty(config.steamUsername))
    {
        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Enter steam username");
        config.steamUsername = Console.ReadLine();
    }

    if (string.IsNullOrEmpty(config.steamPassword))
    {
        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Enter steam password");
        config.steamPassword = Console.ReadLine();
    }

    Server s = new Server(config);
    s.BackupServerData();
    s.UpdateServer();
    s.UpdateAndMoveMods(true, true);

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
            serverConfig.template = config.missionName;
            serverConfig.instanceId = config.instanceId;
            serverConfig.steamQueryPort = config.steamQueryPort;
            using (StreamWriter writer = new StreamWriter(Path.Combine(config.serverPath, config.serverConfigName)))
            {
                string serverConfigText = ServerConfigSerializer.Serialize(serverConfig);
                writer.Write(serverConfigText);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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
            serverConfig.template = config.missionName;
            serverConfig.instanceId = config.instanceId;
            using (StreamWriter writer = new StreamWriter(Path.Combine(config.serverPath, config.serverConfigName)))
            {
                string serverConfigText = ServerConfigSerializer.Serialize(serverConfig);
                writer.Write(serverConfigText);
                writer.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
        }
    }

    AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

    s.StartServer();
    s.StartBEC();
    Thread.Sleep(30000);

    int i = 20;
    bool kill = false;
    while (!kill)
    {
        if (!s.CheckServer())
        {
            s.BackupServerData();
            s.UpdateServer();
            s.UpdateAndMoveMods(false, true);
            s.StartServer();
        }
        else
        {
            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} The Server is still running");
        }
        if (!s.CheckBEC())
        {
            s.StartBEC();
        }
        else
        {
            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} BEC is still running");
        }
        if (i % 40 == 0)
        {
            s.UpdateAndMoveMods(true, false);
            s.CheckForUpdatedMods();
        }
        i++;
        Thread.Sleep(30000);
    }
    s.KillServerProcesses();

    void OnProcessExit(object? sender, EventArgs? e)
    {
        s.KillServerProcesses();
    }
}

void UpdateConfig(Config config)
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
        Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
    }
}