using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");


string configName = "Config.json";

if (FileSystem.FileExists(configName))
{
    Config? config = DeserializeManagerConfig(configName);

    if (config != null)
    {
        Server dayZServer = new Server(config);
        Manager.config = config;
        Manager.server = dayZServer;
        if (config.autoStartServer)
        {
            Manager.StartServer();
        }
    }
}
else
{
    Config config = new Config();
    Server dayZServer = new Server(config);
    Manager.config = config;
    Manager.server = dayZServer;
    SetStandardConfig(config);
    SerializeManagerConfig(config);
}

AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

app.Run();

void OnProcessExit(object? sender, EventArgs? e)
{
    Manager.KillServerProcesses();
}

Config? DeserializeManagerConfig(string name)
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

void SerializeManagerConfig(Config config)
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


void WriteToConsole(string message)
{
    Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
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
    config.restartOnUpdate = StandardManagerConfig.RESTARTONUPDATE;
    config.restartInterval = StandardManagerConfig.RESTARTINTERVAL;
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
