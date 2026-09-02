using System.Diagnostics;
using System.Net;
using Application.IRepository;
using Application.IService;
using Domain.Constants;
using Domain.Manager;
using Domain.Profile;
using Domain.ServerConfig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class ServerInstance : IServerInstance
{
    public bool IsRunning { get; private set; }
    
    private readonly ILogger<ServerInstance> _logger;
    private readonly int _id;
    
    // Other Variables
    private bool _updatedMods;
    private bool _updatedServer;
    private bool _restartingForUpdates;
    private List<long> _updatedModIds;
    private string _battlEyeFolderPath;
    private string _profilePath;

    private Timer? _serverLoopTimer;
    private Timer? _serverUpdateTimer;

    private Process? _serverProcess;
    
    private Task? connectTask;
    
    // Eigene Scoped-Dependencies pro Server
    private readonly IServiceScope _serverScope;
    
    private readonly ServerInformation _serverInformation;
    
    private readonly ISteamCmdService _steamCmdService;
    private readonly ISchedulerService? _scheduler;
    
    public ServerConfig ServerConfig { get; set; }
    
    public bool MissionNeedsUpdating { get; set; }
    
    public ServerInstance(ILogger<ServerInstance> logger, int id, IServiceScopeFactory scopeFactory, ISteamCmdService steamCmdService)
    {
        _logger = logger;
        _id = id;
        _serverScope = scopeFactory.CreateScope();
        var instanceConfig = GetInstanceConfig();
        _serverInformation = new ServerInformation();
        _steamCmdService = steamCmdService;
        _profilePath = Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder, instanceConfig.profileName);
        _battlEyeFolderPath = OperatingSystem.IsWindows() ? Path.Combine(_profilePath, Folders.BattleyeFolderName) : Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder, Folders.BattleyeFolderName);
        _scheduler = _serverScope.ServiceProvider.GetService<ISchedulerService>();
        
        _serverInformation.managerStatus = Statuses.Listening;
        _serverInformation.dayzServerStatus = Statuses.NotRunning;
        
        var serverRepository = _serverScope.ServiceProvider.GetService<IServerRepository>();
        serverRepository?.CreateFoldersAndFiles(Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder), instanceConfig.profileName, _battlEyeFolderPath);

        var serverConfigService = _serverScope.ServiceProvider.GetService<IServerConfigService>();

        ServerConfig = 
            serverConfigService?.Get(Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder, instanceConfig.serverConfigName)) ??
            new ServerConfig();
        
        UpdateServerConfig(instanceConfig);
        
        _serverProcess = null;
        _updatedModIds = [];
        _updatedMods = false;
        _updatedServer = false;
        _restartingForUpdates = false;
        MissionNeedsUpdating = false;
    }
    
    public void StartTimer(string steamUsername, string steamPassword)
    {
        // Server-Logik initialisieren
        IsRunning = true;
        
        var instanceConfig = GetInstanceConfig();
            
        if (string.IsNullOrEmpty(steamUsername) || string.IsNullOrEmpty(steamPassword))
        {
            _serverInformation.managerStatus = Statuses.Credentials;
            return;
        }
        
        UpdateServerConfig(instanceConfig);

        _steamCmdService.WaitForSteamCmd();
        
        CheckForUpdates(instanceConfig);
        MoveAndBackupServer(instanceConfig);

        _serverInformation.managerStatus = Statuses.StartingServer;
        
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var whitelistedPlayerUids = playerRepository?.GetWhitelistedPlayerNames(instanceConfig.id);
        if (whitelistedPlayerUids != null) _scheduler?.SaveWhitelistedPlayers(Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder), whitelistedPlayerUids);
        
        StartServer(instanceConfig);

        _serverInformation.managerStatus = Statuses.StartingScheduler;
        _scheduler?.Disconnect();
        StartScheduler(instanceConfig);

        _serverInformation.dayzServerStatus = Statuses.Started;

        _serverLoopTimer = new Timer(ServerLoop, null , TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        _serverUpdateTimer = new Timer(UpdateLoop, null , TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    public void Stop()
    {
        IsRunning = false;
        KillServerProcesses();
    }
    
    public void Dispose()
    {
        _serverScope.Dispose();
    }

    public ServerInformation GetServerInformation()
    {
        var schedulerInformation = _scheduler?.GetSchedulerInformation();
        _serverInformation.players = schedulerInformation?.Players ?? [];
        _serverInformation.playersCount = schedulerInformation?.PlayersCount ?? 0;
        _serverInformation.chatLog = schedulerInformation?.ChatLog ?? "";
        return _serverInformation;
    }

    private Instance GetInstanceConfig()
    {
        var instanceRepository = _serverScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.GetInstance(_id) ?? new Instance();
    }

    private void ServerLoop(object? state)
    {
        var instanceConfig = GetInstanceConfig();
        
        if (!CheckServer())
        {
            _profilePath = Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder, instanceConfig.profileName);
            _battlEyeFolderPath = OperatingSystem.IsWindows() ? Path.Combine(_profilePath, Folders.BattleyeFolderName) : Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder, Folders.BattleyeFolderName);

            MoveAndBackupServer(instanceConfig);

            var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();

            var whitelistedPlayerUids = playerRepository?.GetWhitelistedPlayerNames(instanceConfig.id);
            if (whitelistedPlayerUids != null) _scheduler?.SaveWhitelistedPlayers(Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder), whitelistedPlayerUids);

            UpdateServerConfig(instanceConfig);

            StartServer(instanceConfig);
        }
        else
        {
            var playerCount = _scheduler?.UpdatePlayers(instanceConfig.id);
            _logger.LogInformation($"The Server is still running with {playerCount} players playing on it");
        }

        if (!CheckScheduler())
        {
            _scheduler?.Disconnect();
            StartScheduler(instanceConfig);
        }
        else
        {
            _logger.LogInformation("Scheduler is still running");
        }
        _serverInformation.managerStatus = Statuses.Listening;
    }

    private void UpdateLoop(object? state)
    {
        var instanceConfig = GetInstanceConfig();
        CheckForUpdates(instanceConfig);
        RestartForUpdates(instanceConfig);
    }

    private void CheckForUpdates(Instance instance)
    {
        List<Mod> mods =
        [
            .. instance.clientMods.Select(x => x.Mod),
            .. instance.serverMods.Select(x => x.Mod)
        ];
        
        var serverRepository = _serverScope.ServiceProvider.GetService<IServerRepository>();
        var missionNeedsUpdating = false;
        _updatedModIds = serverRepository?.CheckForUpdates(mods, Path.Combine(Folders.ServersFolderName, instance.serverFolder), out _updatedMods, out missionNeedsUpdating, out _updatedServer) ?? [];
        MissionNeedsUpdating = missionNeedsUpdating;
    }
    
    private bool CheckScheduler()
    {
        try
        {
            if (!(_serverProcess != null && _restartingForUpdates))
            {
                return connectTask is { IsCompleted: false } || (_scheduler?.IsConnected() ?? false);
            }
            else if (_serverProcess != null && _restartingForUpdates)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when checking scheduler");
            return false;
        }
    }

    private void StartScheduler(Instance instance)
    {
        try
        {
            var onlyRestarts = instance.clientMods.FindAll(mod => mod.Mod.name.Contains(SteamCmd.ExpansionModSearch, StringComparison.CurrentCultureIgnoreCase)).Count > 0;

            _scheduler?.InitializeScheduler(instance.id, Urls.Localhost, instance.RConPort, instance.RConPassword,
                instance.restartInterval, onlyRestarts, instance.customMessages,
                Path.Combine(Folders.ServersFolderName, instance.serverFolder));
            
            connectTask = new Task(() => { _scheduler?.Connect(); });
            connectTask.Start();
            _scheduler?.UpdatePlayers(instance.id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when starting scheduler");
        }
    }

    private void StartServer(Instance instance)
    {
        _serverInformation.chatLog = "";

        _updatedModIds = [];
        _updatedMods = false;
        _restartingForUpdates = false;
        _updatedServer = false;
        var clientModsToLoad = string.Empty;
        foreach (var clientMod in instance.clientMods)
        {
            clientModsToLoad += clientMod.Mod.name + ";";
        }
        if (!string.IsNullOrEmpty(clientModsToLoad))
        {
            clientModsToLoad = $"\"-mod={clientModsToLoad.Remove(clientModsToLoad.Length - 1)}\"";
        }

        var serverModsToLoad = string.Empty;
        foreach (var serverMod in instance.serverMods)
        {
            serverModsToLoad += serverMod.Mod.name + ";";
        }
        if (!string.IsNullOrEmpty(serverModsToLoad))
        {
            serverModsToLoad = $"\"-serverMod={serverModsToLoad.Remove(serverModsToLoad.Length - 1)}\"";
        }

        try
        {
            _serverProcess = new Process();
            var procInf = new ProcessStartInfo();
            var startParameters = GetServerStartParameters(clientModsToLoad, serverModsToLoad, instance);
            procInf.WorkingDirectory = Path.Combine(Folders.ServersFolderName, instance.serverFolder);
            procInf.Arguments = startParameters;
            procInf.FileName = Path.Combine(Folders.ServersFolderName, instance.serverFolder, Files.ServerExecutableFileName);
            _serverProcess.StartInfo = procInf;
            _logger.LogInformation(Statuses.StartingServer);
            _serverProcess.Start();
            _serverInformation.dayzServerStatus = Statuses.Running;
            _logger.LogInformation($"Server starting at {Path.Combine(Folders.ServersFolderName, instance.serverFolder, Files.ServerExecutableFileName)} with the parameters {startParameters}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when starting server process");
        }
    }

    private string GetServerStartParameters(string clientModsToLoad, string serverModsToLoad, Instance instance)
    {
        var parameters = "";
        parameters = $"-instanceId={instance.id} \"-config={instance.serverConfigName}\" \"-profiles={instance.profileName}\" -port={instance.serverPort} {clientModsToLoad} {serverModsToLoad} -cpuCount={instance.cpuCount}";

        if (instance.noFilePatching)
        {
            parameters += " -noFilePatching";
        }
        if (instance.doLogs)
        {
            parameters += " -doLogs";
        }
        if (instance.adminLog)
        {
            parameters += " -adminLog";
        }
        if (instance.freezeCheck)
        {
            parameters += " -freezeCheck";
        }
        if (instance.netLog)
        {
            parameters += " -netLog";
        }
        if (instance.limitFPS > 0)
        {
            parameters += $" -limitFPS={instance.limitFPS}";
        }

        return parameters;
    }

    public bool CheckServer()
    {
        try
        {
            if (_serverProcess is { HasExited: false })
            {
                _serverInformation.dayzServerStatus = Statuses.Running;
                return true;
            }
            else
            {
                _serverInformation.dayzServerStatus = Statuses.NotRunning;
                _serverProcess = null;
                return false;
            }
        }
        catch (Exception ex)
        {
            _serverInformation.dayzServerStatus = Statuses.NotRunning;
            _logger.LogError(ex, "Error when accessing getting server status");
            _serverProcess = null;
            return false;
        }
    }

    public HttpStatusCode BanPlayer(string playerGuid, int instanceId, string reason, int duration)
    {
        return _scheduler?.BanPlayer(playerGuid, instanceId, reason, duration) ?? HttpStatusCode.InternalServerError;
    }

    public HttpStatusCode UnbanPlayer(string playerGuid, int instanceId)
    {
        return _scheduler?.UnbanPlayer(playerGuid, instanceId) ?? HttpStatusCode.InternalServerError;
    }

    public void KickPlayer(string playerGuid, int instanceId, string reason)
    {
        _scheduler?.KickPlayer(playerGuid, instanceId, reason);
    }

    public HttpStatusCode WhitelistPlayer(string playerGuid, int instanceId)
    {
        return _scheduler?.WhitelistPlayer(playerGuid, instanceId) ?? HttpStatusCode.InternalServerError;
    }

    public HttpStatusCode UnwhitelistPlayer(string playerGuid, int instanceId)
    {
        return _scheduler?.UnwhitelistPlayer(playerGuid, instanceId) ?? HttpStatusCode.InternalServerError;
    }
    
    private void MoveAndBackupServer(Instance instance)
    {
        List<Mod> mods =
        [
            .. instance.clientMods.Select(x => x.Mod),
            .. instance.serverMods.Select(x => x.Mod)
        ];

        var serverRepository = _serverScope.ServiceProvider.GetService<IServerRepository>();

        var hasExpansion = instance.clientMods
            .FindAll(p => p.Mod.name.ToLower().Contains(SteamCmd.ExpansionModSearch)).Count > 0;

        if (instance.makeBackups)
        {
            _serverInformation.managerStatus = Statuses.BackingUpServer;
            _logger.LogInformation(Statuses.BackingUpServer);
            serverRepository?.BackupServerData(instance.deleteBackups,
                Path.Combine(Folders.BackupsFolderName, instance.backupPath), instance.profileName,
                instance.missionName, instance.maxKeepTime,
                Path.Combine(Folders.ServersFolderName, instance.serverFolder));
            _logger.LogInformation(Statuses.ServerBackedUp);
            _serverInformation.managerStatus = Statuses.ServerBackedUp;
        }
        
        if (_updatedServer)
        {
            _serverInformation.managerStatus = Statuses.MovingServer;
            _logger.LogInformation(Statuses.MovingServer);
            
            serverRepository?.MoveServer(Path.Combine(Folders.ServersFolderName, instance.serverFolder), instance.profileName, instance.serverConfigName);

            _serverInformation.managerStatus = Statuses.ServerMoved;
            _logger.LogInformation(Statuses.ServerMoved);
        }

        if (_updatedMods)
        {
            _serverInformation.managerStatus = Statuses.MovingMods;
            _logger.LogInformation(Statuses.MovingMods);
            
            serverRepository?.MoveMods(mods, _updatedModIds, Path.Combine(Folders.ServersFolderName, instance.serverFolder));

            _serverInformation.managerStatus = Statuses.ModsMoved;
            _logger.LogInformation(Statuses.ModsMoved);
        }

        if (_updatedServer || MissionNeedsUpdating)
        {
            _updatedMods = false;
            _updatedServer = false;
            MissionNeedsUpdating = false;
            
            _logger.LogInformation(Statuses.UpdatingMission);
            _serverInformation.managerStatus = Statuses.UpdatingMission;

            var missionService = _serverScope.ServiceProvider.GetService<IMissionService>();
            missionService?.UpdateMission(Path.Combine(Folders.ServersFolderName, instance.serverFolder),
                instance.missionName, instance.missionTemplateName, instance.vanillaMissionName,
                Path.Combine(Folders.BackupsFolderName, instance.backupPath),
                instance.mapName, hasExpansion);
            
            _logger.LogInformation(Statuses.MissionUpdated);
            _serverInformation.managerStatus = Statuses.MissionUpdated;

        }

        if (hasExpansion)
        {
            var notFile =
                serverRepository?.UpdateExpansionNotificationFile(
                    Path.Combine(Folders.ServersFolderName, instance.serverFolder), instance.profileName) ??
                new NotificationSchedulerFile(1, 1, 0, 0, new List<NotificationItem>());
            
            var restartUpdater = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
            restartUpdater?.UpdateExpansionScheduler(instance, notFile);
        }
    }

    public void KillServerProcesses()
    {
        var instanceConfig = GetInstanceConfig();

        try
        {
            _serverUpdateTimer?.Dispose();
            _serverUpdateTimer = null;
            _serverLoopTimer?.Dispose();
            _serverLoopTimer = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when killing steamCmd");
        }
        
        try
        {
            if (CheckServer())
            {
                _serverInformation.dayzServerStatus = Statuses.StoppingServer;
                
                _scheduler?.Shutdown();
                _scheduler?.Disconnect();
                
                Thread.Sleep(5000);
                
                _serverProcess?.Kill();
                _serverProcess = null;
                
                Thread.Sleep(5000);
                
                _serverInformation.dayzServerStatus = Statuses.NotRunning;

                _profilePath = Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder, instanceConfig.profileName);
                _battlEyeFolderPath = OperatingSystem.IsWindows() ? Path.Combine(_profilePath, Folders.BattleyeFolderName) : Path.Combine(Folders.ServersFolderName, instanceConfig.serverFolder, Folders.BattleyeFolderName);
                
                UpdateServerConfig(instanceConfig);
            }
            else
            {
                _scheduler?.Disconnect();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when killing server and ajdusting and saving the server config");
        }
    }

    private bool RestartForUpdates(Instance instance)
    {
        if (instance.restartOnUpdate && !_restartingForUpdates && ((_updatedMods && _updatedModIds.Count > 0) || _updatedServer))
        {
            try
            {
                var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
                
                if (restartUpdaterService?.IsTimeToRestart(instance.restartInterval) ?? false)
                {
                    _restartingForUpdates = true;
                    _scheduler?.ChangeToUpdateMode();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when changing to update mode");
                return false;
            }
        }
        else
        {
            _updatedMods = false;
            _updatedServer = false;
        }
        return false;
    }

    private void UpdateServerConfig(Instance instance)
    {
        var serverRepository = _serverScope.ServiceProvider.GetService<IServerRepository>();
        serverRepository?.UpdateBeConfigs(_battlEyeFolderPath, instance.RConPassword, instance.RConPort);
        var serverConfigService = _serverScope.ServiceProvider.GetService<IServerConfigService>();
        serverConfigService?.UpdateServerConfig(ServerConfig, instance.missionName, instance.hostName, instance.id, instance.steamPort, instance.steamQueryPort);
        serverConfigService?.Save(ServerConfig, Path.Combine(Folders.ServersFolderName, instance.serverFolder, instance.serverConfigName));
    }
}