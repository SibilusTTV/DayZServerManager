using System.Net;
using System.Text.RegularExpressions;
using Application.Handlers;
using Application.IRepository;
using Application.IService;
using Domain.Constants;
using Domain.Manager;
using Domain.Profile;
using Domain.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class SchedulerService : ISchedulerService
{
    private readonly ILogger<SchedulerService> _logger;
    private readonly IServiceScope _serverScope;
    private readonly IRconRepository _rconRepository;
    
    private Timer? _autoLoadBansTimer;
    private List<JobTimer> _automaticMessages;
    private List<JobTimer> _customMessages;
    
    private string _adminLog;
    private bool _onlyRestarts;
    private int _instanceId;

    public SchedulerService(ILogger<SchedulerService> logger, IRconRepository rconRepository, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _rconRepository = rconRepository;
        _serverScope = scopeFactory.CreateScope();
        _automaticMessages = [];
        _customMessages = [];
        _adminLog = "";
        _onlyRestarts = false;
        _instanceId = 0;
    }

    public void InitializeScheduler(int instanceId, string ip, int port, string password,
        bool onlyRestarts, string serverFolderName)
    {
        _instanceId = instanceId;
        var config = Get(instanceId);

        if (config == null)
        {
            config = new SchedulerConfig(instanceId);
            CreateEdit(config);
        }
        
        if (config.restartInterval is < 1 or > 24)
        {
            throw new Exception("The interval needs to be between 1 and 24");
        }

        GetWhitelistedPlayers(serverFolderName);

        var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
        _automaticMessages = restartUpdaterService?.CreateSchedule(false, _onlyRestarts, config.restartInterval, SendCommand, IsConnected) ?? [];
        _customMessages = restartUpdaterService?.CreateCustomJobTimers(_onlyRestarts, config.restartInterval, SendCommand, IsConnected, config.customMessages) ?? [];

        _rconRepository.InitializeRconRepository(ip, port, password);
    }
    
    public bool Connect()
    {
        var config = Get(_instanceId);
        _logger.LogInformation($"Waiting for {config?.Timeout} seconds until TimeOut is over");
        Thread.Sleep(config?.Timeout * 1000 ?? 10000);
        _logger.LogInformation("Connecting to the Server");
        
        if (!(_rconRepository?.Connect() ?? false))
        {
            Disconnect();
            return false;
        }

        return true;
    }

    public void Disconnect()
    {
        if (IsConnected())
        {
            _rconRepository?.Disconnect();
        }

        _autoLoadBansTimer?.Dispose();
        _autoLoadBansTimer = null;
        KillCustomTasks();
        KillAutomaticTasks();
    }

    public SchedulerInformation GetSchedulerInformation()
    {
        return new SchedulerInformation()
        {
            Players = _rconRepository.ConnectedPlayers,
            PlayersCount = _rconRepository.ConnectedPlayers.Count,
            ChatLog = _rconRepository.ChatLog,
            AdminLog = _adminLog,
        };
    }

    public List<string> GetWhitelistedPlayers(string serverFolderName)
    {
        var schedulerRepository = _serverScope.ServiceProvider.GetService<ISchedulerRepository>();
        return schedulerRepository?.LoadWhitelistedPlayers(serverFolderName) ?? [];
    }

    public HttpStatusCode SaveWhitelistedPlayers(string serverFolderName, List<string> whitelistedPlayers)
    {
        var schedulerRepository = _serverScope.ServiceProvider.GetService<ISchedulerRepository>();
        return schedulerRepository?.SaveWhitelistedPlayers(serverFolderName, whitelistedPlayers) ?? HttpStatusCode.InternalServerError;
    }

    public void ChangeToNormalMode()
    {
        KillAutomaticTasks();
        var config = Get(_instanceId);
        if (config == null) return;
        var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
        _automaticMessages = restartUpdaterService?.CreateSchedule(false, _onlyRestarts, config.restartInterval, SendCommand, IsConnected) ?? [];
    }

    public void ChangeToUpdateMode()
    {
        KillAutomaticTasks();
        var config = Get(_instanceId);
        if (config == null) return;
        var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
        _automaticMessages = restartUpdaterService?.CreateSchedule(true, _onlyRestarts, config.restartInterval, SendCommand, IsConnected) ?? [];
    }

    public void KillAutomaticTasks()
    {
        foreach (var timer in _automaticMessages)
        {
            timer.Dispose();
        }
        _automaticMessages.Clear();
    }

    public void KillCustomTasks()
    {
        foreach (var timer in _customMessages)
        {
            timer.Dispose();
        }
        _customMessages.Clear();
    }

    public bool IsConnected()
    {
        return _rconRepository.IsConnected();
    }

    public int UpdatePlayers(int instanceId)
    {
        if (!IsConnected()) return 0;
        
        var instance = _serverScope.ServiceProvider.GetService<IInstanceRepository>()?.GetInstance(instanceId);
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        
        if (instance == null) return 0;
        
        var playerUids = GetAdminLog(instance);
        var playerPermissions = ReadOutRolesAndPlayers(instanceId);
        var newPlayers = _rconRepository.GetPlayers();

        foreach (var player in newPlayers)
        {
            if (!playerUids.TryGetValue(player.Name, out var uid) ||
                !playerPermissions.TryGetValue(uid, out var playerPermission))
            {
                _rconRepository.ConnectedPlayers.RemoveAll(x => x.Guid == player.Guid);
                continue;
            }

            var newPlayer = new User(player.Guid, player.Name, uid, player.IsVerified, player.Ip);
            playerRepository?.CreateEditPlayer(newPlayer);
            
            var serverPlayer = playerRepository?.GetServerPlayerByGuid(player.Guid, instanceId);
            
            var roleName = playerPermission.Roles.FirstOrDefault();
            if (roleName == null) continue;
            
            var role = playerRepository?.GetRole(roleName, instanceId);
            if (role == null) continue;

            if (serverPlayer == null)
            {
                serverPlayer = new ServerPlayer(instanceId, player.Guid, false, false, role.Id);
            }
            else
            {
                serverPlayer.RoleId = role.Id;
                serverPlayer.Role = role;
            }
            
            playerRepository?.CreateEditServerPlayer(serverPlayer);
        }
        
        _rconRepository.ReloadBans();
        var bans = _rconRepository.GetBans();
        var bannedPlayers = playerRepository?.GetBannedServerPlayers(instanceId);
        
        foreach (var ban in bans)
        {
            var serverPlayer = playerRepository?.GetServerPlayerByGuid(ban.Guid, instanceId);
            if (serverPlayer == null || bannedPlayers != null && bannedPlayers.Any(x => x.User.Guid == ban.Guid)) continue;
            serverPlayer.IsBanned = true;
            playerRepository?.CreateEditServerPlayer(serverPlayer);
        }
        
        if (bannedPlayers == null) return _rconRepository.ConnectedPlayers.Count;
        foreach (var bannedPlayer in bannedPlayers)
        {
            if (bans.Any(x => x.Guid == bannedPlayer.User.Guid)) continue;

            bannedPlayer.IsBanned = false;
            playerRepository?.CreateEditServerPlayer(bannedPlayer);
        }

        return _rconRepository.ConnectedPlayers.Count;
    }

    public void KickPlayer(string guid, int instanceId, string reason)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var player = playerRepository?.GetServerPlayerByGuid(guid, instanceId);
        
        if (player == null) return;
        
        var connectedPlayer = _rconRepository.ConnectedPlayers.Find(x => x.Guid == guid);
        if (connectedPlayer != null)
        {
            _rconRepository.KickPlayer(connectedPlayer.Id, reason, player.User.Name);
        }
    }

    public HttpStatusCode BanPlayer(string playerGuid, int instanceId, string reason, int duration)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var serverPlayer = playerRepository?.GetServerPlayerByGuid(playerGuid, instanceId);
        if (serverPlayer == null) return HttpStatusCode.NotFound;
        
        var code = _rconRepository.BanPlayer(serverPlayer.User.Guid, reason, duration, serverPlayer.User.Name);
        serverPlayer.IsBanned = true;
        return playerRepository?.CreateEditServerPlayer(serverPlayer) ?? code;
    }

    public HttpStatusCode UnbanPlayer(string playerGuid, int instanceId)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var serverPlayer = playerRepository?.GetServerPlayerByGuid(playerGuid, instanceId);
        if (serverPlayer == null) return HttpStatusCode.NotFound;
        
        var bans = _rconRepository.GetBans();
        var playerBan = bans.FirstOrDefault(x => x.Guid == serverPlayer.User.Guid);
        if (playerBan == null) return HttpStatusCode.NotFound;
        
        var code = _rconRepository.UnbanPlayer(playerBan.Id, serverPlayer.User.Name);
        serverPlayer.IsBanned = false;
        return playerRepository?.CreateEditServerPlayer(serverPlayer) ?? code;
    }

    public HttpStatusCode WhitelistPlayer(string playerGuid, int instanceId)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var serverPlayer = playerRepository?.GetServerPlayerByGuid(playerGuid, instanceId);
        if (serverPlayer == null)
        {
            var player = playerRepository?.GetPlayer(playerGuid);
            if (player == null) return HttpStatusCode.NotFound;
            
            var role = playerRepository?.GetRole("everyone", instanceId);
            if (role == null)
            {
                role = playerRepository?.GetRoles(instanceId).FirstOrDefault();
                if (role == null) return HttpStatusCode.NotFound;
            }
            
            serverPlayer = new ServerPlayer(instanceId, playerGuid, true, false, role.Id);
        }
        
        var instanceRepository = _serverScope.ServiceProvider.GetService<IInstanceRepository>();
        var instance = instanceRepository?.GetInstance(instanceId);
        if (instance == null) return HttpStatusCode.NotFound;
        
        var whitelistedPlayers = GetWhitelistedPlayers(Path.Combine(Folders.ServersFolderName, instance.serverFolder));

        playerRepository?.WhitelistPlayer(serverPlayer.Id);
        if (!whitelistedPlayers.Contains(playerGuid))
        {
            whitelistedPlayers.Add(playerGuid);
        }

        _logger.LogInformation($"{serverPlayer.User.Name} was whitelisted");
        return SaveWhitelistedPlayers(Path.Combine(Folders.ServersFolderName, instance.serverFolder), whitelistedPlayers);
    }

    public HttpStatusCode UnwhitelistPlayer(string playerGuid, int instanceId)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var player = playerRepository?.GetServerPlayerByGuid(playerGuid, instanceId);
        
        if (player == null) return HttpStatusCode.NotFound;
        
        var instanceService = _serverScope.ServiceProvider.GetService<IInstanceService>();
        var instance = instanceService?.GetInstance(player.InstanceId);
        
        if (instance == null) return HttpStatusCode.NotFound;
        
        var whitelistedPlayers = GetWhitelistedPlayers(Path.Combine(Folders.ServersFolderName, instance.serverFolder));
        
        playerRepository?.UnWhitelistPlayer(player.Id);
        if (whitelistedPlayers.Contains(player.User.Uid))
        {
            whitelistedPlayers.Remove(player.User.Uid);
        }

        _logger.LogInformation($"{player.User.Name} was unwhitelisted");
        return SaveWhitelistedPlayers(Path.Combine(Folders.ServersFolderName, instance.serverFolder), whitelistedPlayers);
    }

    public void SendCommand(string command)
    {
        _rconRepository.SendCommand(command);
    }

    public void Shutdown()
    {
        _rconRepository.Shutdown();
    }

    public SchedulerConfig? Get(int instanceId)
    {
        var schedulerRepository = _serverScope.ServiceProvider.GetService<ISchedulerRepository>();
        return schedulerRepository?.Get(instanceId);
    }

    public void CreateEdit(SchedulerConfig schedulerConfig)
    {
        var schedulerRepository = _serverScope.ServiceProvider.GetService<ISchedulerRepository>();
        schedulerRepository?.CreateEdit(schedulerConfig);
    }

    public bool RestartForUpdates(int instanceId, bool restartingForUpdates, int updatedModsCount, bool updatedServer)
    {
        var config = Get(instanceId);
        if (config == null) return false;
        
        if (config.restartOnUpdate && !restartingForUpdates && (updatedModsCount > 0 || updatedServer))
        {
            try
            {
                var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
                
                if (restartUpdaterService?.IsTimeToRestart(config.restartInterval) ?? false)
                {
                    ChangeToUpdateMode();
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
        return false;
    }

    private Dictionary<string, string> GetAdminLog(Instance instance)
    {
        var serverRepository = _serverScope.ServiceProvider.GetService<IServerRepository>();
        
        var returnString = serverRepository?.GetAdminLog(Path.Combine(Folders.ServersFolderName, instance.serverFolder), instance.profileName);

        if (_adminLog == returnString || returnString == null) return [];
        
        var pattern = @"Player ""(?'name'[^\n]+)"" \(id=(?'id'\S*)=\)";
        var regex = new Regex(pattern);
        var matches = regex.Matches(returnString);

        Dictionary<string, string> playerUids = new Dictionary<string, string>();
        
        foreach (Match match in matches)
        {
            var name = match.Groups["name"].Value;
            var uid = match.Groups["id"].Value;

            playerUids.TryAdd(name, uid);
        }
        
        _adminLog = returnString;
        return playerUids;
    }

    private Dictionary<string, PlayerPermissions> ReadOutRolesAndPlayers(int id)
    {
        var playerService = _serverScope.ServiceProvider.GetService<IPlayerService>();
        playerService?.ReadOutRoles(id);
        return playerService?.ReadOutServerPlayerRoles(id) ?? new Dictionary<string, PlayerPermissions>();
    }
}