using System.Collections.Concurrent;
using System.Net;
using Application.IRepository;
using Application.IService;
using Domain.Manager;
using Domain.ServerConfig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class InstanceService : IInstanceService, IDisposable
{
    private readonly ConcurrentDictionary<Guid, IServerInstance> _servers 
        = new();
    private readonly ILogger<InstanceService> _logger;
    private readonly IServiceScope _serviceScope;
    private readonly IServerFactory _serverFactory;
    private readonly ISteamCmdService _steamCmdService;
    
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
        
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        var instances = instanceRepository?.GetInstances();

        if (instances == null) return;
        if (!credentialsNotSet && instances.Count > 0) StartSteamCmdService();
        
        foreach (var instance in instances)
        {
            CreateServer(instance);
        
            if (instance.autoStartServer && !credentialsNotSet)
            {
                StartServer(instance.id);
            }
        }
    }
    
    public IServerInstance CreateServer(Instance instanceConfig)
    {
        if (_servers.ContainsKey(instanceConfig.id))
        {
            throw new InvalidOperationException($"Server {instanceConfig.id} existiert bereits");
        }
        
        var server = _serverFactory.CreateServerAsync(instanceConfig.id);
        _servers[instanceConfig.id] = server;

        if (!_steamCmdService.CheckUpdateLoop())
        {
            StartSteamCmdService();
        }
        
        _logger.LogInformation("Server {instanceConfig.id} erstellt", instanceConfig.id);
        return server;
    }

    public HttpStatusCode CreateInstance(Instance instanceConfig)
    {
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.CreateInstance(instanceConfig) ?? HttpStatusCode.InternalServerError;
    }
    
    public IServerInstance? GetServer(Guid id)
    {
        return _servers.GetValueOrDefault(id);
    }

    public Instance? GetInstance(Guid id)
    {
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.GetInstance(id);
    }

    public List<Instance> GetInstances()
    {
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.GetInstances() ?? [];
    }

    public ServerInformation GetServerInformation(Guid id)
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

        List<Mod> clientMods = [cfMod, cotMod];
        
        return new Instance(nextInstanceId, serverFolder, steamPort, serverPort, steamQueryPort, rConPort, clientMods);
    }

    public HttpStatusCode UpdateInstanceConfig(Instance instanceConfig)
    {
        var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
        return instanceRepository?.UpdateInstance(instanceConfig) ?? HttpStatusCode.NotFound;
    }

    public ServerConfig GetServerConfig(Guid id)
    {
        var server = GetServer(id);
        return server != null ? server.ServerConfig : new ServerConfig();
    }

    public HttpStatusCode SaveServerConfig(ServerConfig serverConfig, Guid id)
    {
        var instanceConfig = GetInstance(id);

        var serverConfigService = _serviceScope.ServiceProvider.GetService<ServerConfigService>();
        if (serverConfigService != null && instanceConfig != null) serverConfigService.UpdateServerConfig(serverConfig, instanceConfig.missionName, instanceConfig.hostName, instanceConfig.instanceId, instanceConfig.steamPort, instanceConfig.steamQueryPort);
        
        var server = GetServer(id);
        server?.ServerConfig = serverConfig;

        return HttpStatusCode.OK;
    }

    public void SetMissionNeedsUpdatingForServer(Guid id)
    {
        var server = GetServer(id);
        server?.MissionNeedsUpdating = true;
    }

    public void StartServer(Guid id)
    {
        var server = GetServer(id);
        var credentials = GetSteamCredentials();
        
        if (server == null) return;
        
        server.StartTimer(credentials.SteamUsername, credentials.SteamPassword);
    }

    public void StopServer(Guid id)
    {
        var server = GetServer(id);
        server?.Stop();
    }
    
    public void RemoveServer(Guid id)
    {
        if (_servers.TryRemove(id, out var server))
        {
            server.Stop();
            server.Dispose();
            
            var instanceRepository = _serviceScope.ServiceProvider.GetService<IInstanceRepository>();
            
            instanceRepository?.DeleteInstance(id);

            if (instanceRepository?.GetInstances().Count <= 0)
            {
                StopSteamCmdService();
            }
            
            _logger.LogInformation("Server {id} entfernt", id);
        }
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
            if (instances.All(inst => inst.instanceId != id))
            {
                return id;
            }
        }

        return id;
    }
}