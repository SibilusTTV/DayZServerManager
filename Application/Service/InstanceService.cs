using System.Collections.Concurrent;
using System.Net;
using Application.IRepository;
using Application.IService;
using Domain.Manager;
using Domain.Scheduler;
using Domain.ServerConfig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class InstanceService : IInstanceService, IDisposable
{
    private readonly ConcurrentDictionary<int, IServerInstance> _servers 
        = new();
    private readonly ILogger<InstanceService> _logger;
    private readonly IServiceScope _serviceScope;
    private readonly IServerFactory _serverFactory;
    private readonly ISteamCmdService _steamCmdService;
    private Task? _downloadModsTask;
    
    public InstanceService(ILogger<InstanceService> logger,
        IServiceScopeFactory scopeFactory,
        IServerFactory serverFactory,
        ISteamCmdService steamCmdService)
    {
        _logger = logger;
        _serviceScope = scopeFactory.CreateScope();
        _serverFactory = serverFactory;
        _steamCmdService = steamCmdService;
    }

    public void Initialize()
    {
        var managerRepository = _serviceScope.ServiceProvider.GetService<IManagerRepository>();
        
        managerRepository?.CreateFolders();

        var steamCredentials = GetSteamCredentials();
        
        var credentialsNotSet = string.IsNullOrEmpty(steamCredentials.SteamUsername) ||
                                string.IsNullOrEmpty(steamCredentials.SteamPassword);
        
        var instances = GetInstances();

        _logger.LogInformation("Starting SteamCMD");
        if (!credentialsNotSet && instances.Count > 0) StartSteamCmdService();
        
        foreach (var instance in instances)
        {
            CreateServer(instance.id);
        
            if (instance.autoStartServer && !credentialsNotSet)
            {
                StartServer(instance.id);
            }
        }
    }
    
    public IServerInstance CreateServer(int id)
    {
        if (_servers.ContainsKey(id))
        {
            throw new InvalidOperationException($"Server {id} existiert bereits");
        }
        
        var server = _serverFactory.CreateServerAsync(id);
        _servers[id] = server;

        if (!_steamCmdService.CheckUpdateLoop())
        {
            StartSteamCmdService();
        }
        
        _logger.LogInformation("Server {id} erstellt", id);
        return server;
    }

    public HttpStatusCode CreateInstance(Instance instanceConfig)
    {
        DownloadNewMods(instanceConfig);
        
        var schedulerRepository = _serviceScope.ServiceProvider.GetService<ISchedulerRepository>();
        schedulerRepository?.CreateEdit(new SchedulerConfig(instanceConfig.id));
        
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.CreateInstance(instanceConfig) ?? HttpStatusCode.InternalServerError;
    }
    
    public IServerInstance? GetServer(int id)
    {
        return _servers.GetValueOrDefault(id);
    }

    public Instance? GetInstance(int id)
    {
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.GetInstance(id);
    }

    public List<Instance> GetInstances()
    {
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.GetInstances() ?? [];
    }

    public ServerInformation GetServerInformation(int id)
    {
        var server = GetServer(id);
        return server?.GetServerInformation() ?? new ServerInformation();
    }

    public List<ServerInformation> GetServerInformations()
    {
        List<ServerInformation> serverInformations = [];
        
        foreach (var server in _servers.Values)
        {
            serverInformations.Add(server.GetServerInformation());
        }
        
        return serverInformations;
    }

    public Instance? CreateEmptyInstanceConfig()
    {
        var instance = GetInstances();
        
        var nextInstanceId = GetNextId(instance);

        var serverFolder = "server" + nextInstanceId;
        var steamPort = 2201 + (100 * nextInstanceId);
        var serverPort = 2202 + (100 * nextInstanceId);
        var steamQueryPort = 2205 + (100 * nextInstanceId);
        var rConPort = 2206 + (100 * nextInstanceId);

        var modRepository = _serviceScope.ServiceProvider.GetService<IModRepository>();
        var cfMod = modRepository?.GetByWorkshopId(1559212036) ?? new Mod("@CF", 1559212036);
        var cotMod = modRepository?.GetByWorkshopId(1564026768) ?? new Mod("@Community-Online-Tools", 1564026768);

        List<InstanceClientMod> clientMods = [new(nextInstanceId, cfMod, 0), new(nextInstanceId, cotMod, 1)];
        
        return new Instance(nextInstanceId, serverFolder, steamPort, serverPort, steamQueryPort, rConPort, clientMods);
    }

    public HttpStatusCode UpdateInstanceConfig(Instance instanceConfig)
    {
        DownloadNewMods(instanceConfig);
        
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.UpdateInstance(instanceConfig) ?? HttpStatusCode.NotFound;
    }

    public List<PropertyValue> GetServerConfig(int id)
    {
        var server = GetServer(id);
        return server != null ? server.ServerConfig.Properties : [];
    }

    public HttpStatusCode SaveServerConfig(List<PropertyValue> properties, int id)
    {
        var serverConfig = new ServerConfig()
        {
            Properties = properties
        };
        
        var instanceConfig = GetInstance(id);
        
        var serverConfigService = _serviceScope.ServiceProvider.GetService<ServerConfigService>();
        if (serverConfigService != null && instanceConfig != null)
            serverConfigService.UpdateServerConfig(serverConfig, instanceConfig.missionName, instanceConfig.hostName,
                instanceConfig.id, instanceConfig.steamPort, instanceConfig.steamQueryPort);
        
        var server = GetServer(id);
        server?.ServerConfig.Properties = properties;

        return HttpStatusCode.OK;
    }

    public void SetMissionNeedsUpdatingForServer(int id)
    {
        var server = GetServer(id);
        server?.MissionNeedsUpdating = true;
    }

    public HttpStatusCode StartServer(int id)
    {
        var server = GetServer(id);
        var credentials = GetSteamCredentials();
        
        if (server == null) return HttpStatusCode.NotFound;
        
        return server.StartServerLoop(credentials.SteamUsername, credentials.SteamPassword);
    }

    public HttpStatusCode StopServer(int id)
    {
        var server = GetServer(id);
        return server?.Stop() ?? HttpStatusCode.NotFound;
    }
    
    public HttpStatusCode RemoveServer(int id)
    {
        if (_servers.TryGetValue(id, out var serverGet) && serverGet.CheckServer())
        {
            return HttpStatusCode.BadRequest;
        }
        
        if (_servers.TryRemove(id, out var server))
        {
            server.Stop();
            server.Dispose();
            
            var schedulerRepository = _serviceScope.ServiceProvider.GetService<ISchedulerRepository>();
            schedulerRepository?.Delete(id);
            
            var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
            instanceRepository?.DeleteInstance(id);
        
            if (instanceRepository?.GetInstances().Count <= 0)
            {
                StopSteamCmdService();
            }
            
            _logger.LogInformation("Server {id} entfernt", id);
            return HttpStatusCode.OK;
        }
        
        return HttpStatusCode.NotFound;
    }
    
    public IEnumerable<IServerInstance> GetAllServers()
    {
        return _servers.Values;
    }
    
    public void Dispose()
    {
        foreach (var server in _servers.Values)
        {
            server.Stop();
            server.Dispose();
        }
        _steamCmdService.Dispose();
        _servers.Clear();
    }

    public HttpStatusCode BanPlayer(string playerGuid, int instanceId, string reason, int duration)
    {
        var server = GetServer(instanceId);
        return server?.BanPlayer(playerGuid, instanceId, reason, duration) ?? HttpStatusCode.InternalServerError;
    }

    public HttpStatusCode UnbanPlayer(string playerGuid, int instanceId)
    {
        var server = GetServer(instanceId);
        return server?.UnbanPlayer(playerGuid, instanceId) ?? HttpStatusCode.InternalServerError;
    }

    public void KickPlayer(string playerGuid, int instanceId, string reason)
    {
        var server = GetServer(instanceId);
        server?.KickPlayer(playerGuid, instanceId, reason);
    }

    public HttpStatusCode WhitelistPlayer(string playerGuid, int instanceId)
    {
        var server = GetServer(instanceId);
        return server?.WhitelistPlayer(playerGuid, instanceId) ?? HttpStatusCode.InternalServerError;
    }

    public HttpStatusCode UnwhitelistPlayer(string playerGuid, int instanceId)
    {
        var server = GetServer(instanceId);
        return server?.UnwhitelistPlayer(playerGuid, instanceId) ?? HttpStatusCode.InternalServerError;
    }

    public SchedulerConfig? GetSchedulerConfig(int instanceId)
    {
        var server = GetServer(instanceId);
        return server?.GetSchedulerConfig();
    }

    public void CreateEditSchedulerConfig(SchedulerConfig schedulerConfig)
    {
        var server = GetServer(schedulerConfig.InstanceId);
        server?.CreateEditSchedulerConfig(schedulerConfig);
    }

    private void StartSteamCmdService()
    {
        var modRepository = _serviceScope.ServiceProvider.GetService<IModRepository>();
        var mods = modRepository?.GetMods();

        if (mods == null) return;
        
        _steamCmdService.StartTimer();
    }

    private void StopSteamCmdService()
    {
        _steamCmdService.Stop();
    }

    private SteamCredentials GetSteamCredentials()
    {
        var steamCmdRepository = _serviceScope.ServiceProvider.GetService<ISteamCmdRepository>();
        return steamCmdRepository?.GetCredentials() ??  new SteamCredentials();
    }

    private int GetNextId(List<Instance> instances)
    {
        var id = 1;
        for (id = 1; id <= instances.Count; id++)
        {
            if (instances.All(inst => inst.id != id))
            {
                return id;
            }
        }

        return id;
    }

    private void DownloadNewMods(Instance instanceConfig)
    {
        var modsRepository = _serviceScope.ServiceProvider.GetService<IModRepository>();
        var mods = modsRepository?.GetMods().Select(x => x.workshopID).ToHashSet();

        if (mods != null)
        {
            List<Mod> newMods = [];

            foreach (var mod in instanceConfig.clientMods)
            {
                if (!mods.Contains(mod.Mod.workshopID)) newMods.Add(mod.Mod);
            }

            foreach (var mod in instanceConfig.serverMods)
            {
                if (!mods.Contains(mod.Mod.workshopID)) newMods.Add(mod.Mod);
            }
            
            if (newMods.Count <= 0) return;
        
            var steamCmdService = _serviceScope.ServiceProvider.GetService<ISteamCmdService>();
            if (_downloadModsTask != null && !_downloadModsTask.IsCompleted && !_downloadModsTask.IsFaulted &&
                !_downloadModsTask.IsCanceled) return;
            
            _downloadModsTask = new Task(() =>
            {
                steamCmdService?.DownloadMods(newMods);
            });
            _downloadModsTask.Start();
        }
    }
}