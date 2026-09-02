namespace Domain.Manager;

public class Instance
{
    public int id { get; set; }
    public string serverFolder { get; set; }
    public string hostName { get; set; }
    public string missionName { get; set; }
    public string vanillaMissionName { get; set; }
    public string missionTemplateName { get; set; }
    public string serverConfigName { get; set; }
    public string profileName { get; set; }
    public int steamPort { get; set; }
    public int serverPort { get; set; }
    public int steamQueryPort { get; set; }
    public int RConPort { get; set; }
    public string RConPassword { get; set; }
    public int cpuCount { get; set; }
    public bool noFilePatching { get; set; }
    public bool doLogs { get; set; }
    public bool adminLog { get; set; }
    public bool freezeCheck { get; set; }
    public bool netLog { get; set; }
    public int limitFPS { get; set; }
    public string mapName { get; set; }
    public bool restartOnUpdate { get; set; }
    public int restartInterval { get; set; }
    public bool autoStartServer { get; set; }
    public bool makeBackups { get; set; }
    public bool deleteBackups { get; set; }
    public string backupPath { get; set; }
    public int maxKeepTime { get; set; }
    public List<InstanceClientMod> clientMods { get; set; }
    public List<InstanceServerMod> serverMods { get; set; }
    public List<CustomMessage> customMessages { get; set; }

    public Instance()
    {
        
    }
    
    public Instance(int instanceId)
    {
        id = instanceId;
        serverFolder = "server";
        hostName = "Testserver";
        missionName = "Expansion.ChernarusPlus";
        serverConfigName = "serverDZ.cfg";
        profileName = "Profiles";
        steamPort = 2301;
        serverPort = 2302;
        steamQueryPort = 2305;
        RConPort = 2306;
        RConPassword = "YouRConPassword";
        cpuCount = 8;
        noFilePatching = true;
        doLogs = true;
        adminLog = true;
        netLog = true;
        freezeCheck = true;
        limitFPS = -1;
        vanillaMissionName = "dayzOffline.chernarusplus";
        missionTemplateName = "template.chernarus";
        mapName = "Chernarus";
        restartOnUpdate = true;
        restartInterval = 4;
        autoStartServer = false;
        makeBackups = true;
        deleteBackups = true;
        backupPath = "server1";
        maxKeepTime = 7;
        clientMods = new List<InstanceClientMod>();
        serverMods = new List<InstanceServerMod>();
        Mod mod1 = new Mod("@CF", 1559212036);
        clientMods.Add(new InstanceClientMod(instanceId, mod1, 0));
        Mod mod2 = new Mod("@Community-Online-Tools", 1564026768);
        clientMods.Add(new InstanceClientMod(instanceId, mod2, 1));
        customMessages = new List<CustomMessage>();
        customMessages.Add(new CustomMessage(false, new TimeSpan( 0, 5, 0 ), new TimeSpan( 0, 15, 0 ), "Need Help?", "Make sure to join our Discord", "ExclamationMark", ""));
    }
    
    public Instance(int instanceId, string serverFolder, int steamPort, int serverPort, int steamQueryPort, int rConPort, List<InstanceClientMod> clientMods)
    {
        this.id = instanceId;
        this.serverFolder = serverFolder;
        this.hostName = "Testserver " + instanceId;
        missionName = "Expansion.ChernarusPlus";
        serverConfigName = "serverDZ.cfg";
        profileName = "Profiles";
        this.steamPort = steamPort;
        this.serverPort = serverPort;
        this.steamQueryPort = steamQueryPort;
        RConPort = rConPort;
        RConPassword = "YouRConPassword";
        cpuCount = 8;
        noFilePatching = true;
        doLogs = true;
        adminLog = true;
        netLog = true;
        freezeCheck = true;
        limitFPS = -1;
        vanillaMissionName = "dayzOffline.chernarusplus";
        missionTemplateName = "template.chernarus";
        mapName = "Chernarus";
        restartOnUpdate = true;
        restartInterval = 4;
        autoStartServer = false;
        makeBackups = true;
        deleteBackups = true;
        backupPath = "server1";
        maxKeepTime = 7;
        this.clientMods = clientMods;
        serverMods = [];
        customMessages = [];
        customMessages.Add(new CustomMessage(false, new TimeSpan( 0, 5, 0 ), new TimeSpan( 0, 15, 0 ), "Need Help?", "Make sure to join our Discord", "ExclamationMark", ""));
    }
}