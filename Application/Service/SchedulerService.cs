using System.Net;
using Application.Handlers;
using Application.IRepository;
using Application.IService;
using Domain.Constants;
using Domain.Manager;
using Domain.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class SchedulerService : ISchedulerService
{
    private readonly ILogger<SchedulerService> _logger;
    private readonly IServiceScope _serverScope;
    
    private SchedulerConfig? _config;
    private Timer? _autoLoadBansTimer;
    private List<JobTimer> _automaticMessages;
    private List<JobTimer> _customMessages;
    private bool _onlyRestarts;
    private int _interval;
    
    public IRconService? RconClient { get; private set; }

    public SchedulerService(ILogger<SchedulerService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _serverScope = scopeFactory.CreateScope();
        _automaticMessages = [];
        _customMessages = [];
        _onlyRestarts = false;
        _interval = 1;
    }

    public void InitializeScheduler(string ip, int port, string password, int interval, bool onlyRestarts, List<CustomMessage> customMessages, string serverFolderName)
    {
        var rconService = _serverScope.ServiceProvider.GetService<IRconService>();
        if (rconService != null) RconClient = rconService;
        
        _onlyRestarts = onlyRestarts;

        if (interval < 1 && interval > 24)
        {
            throw new Exception("The interval needs to be between 1 and 24");
        }
        else
        {
            _interval = interval;
        }
        
        _autoLoadBansTimer = new Timer((state) => { LoadBans(); }, null, 10000, 10000);

        GetWhitelistedPlayers(serverFolderName);

        var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
        _automaticMessages = restartUpdaterService?.CreateSchedule(false, _onlyRestarts, _interval, SendCommand, IsConnected) ?? [];
        _customMessages = restartUpdaterService?.CreateCustomJobTimers(_onlyRestarts, _interval, SendCommand, IsConnected, customMessages) ?? [];

        RconClient?.InitializeRconService(ip, port, password, _config);
    }
    
    public bool Connect()
    {
        _logger.LogInformation($"Waiting for {_config?.Timeout} seconds until TimeOut is over");
        Thread.Sleep(_config?.Timeout * 1000 ?? 10000);
        _logger.LogInformation("Connecting to the Server");
        
        if (!(RconClient?.Connect() ?? false))
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
            RconClient?.Disconnect();
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
            players = RconClient?.ConnectedPlayers ?? [],
            playersCount = RconClient?.PlayersCount ?? 0,
            chatLog = RconClient?.ChatLog ?? "",
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
        var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
        _automaticMessages = restartUpdaterService?.CreateSchedule(false, _onlyRestarts, _interval, SendCommand, IsConnected) ?? [];
    }

    public void ChangeToUpdateMode()
    {
        KillAutomaticTasks();
        var restartUpdaterService = _serverScope.ServiceProvider.GetService<IRestartUpdaterService>();
        _automaticMessages = restartUpdaterService?.CreateSchedule(true, _onlyRestarts, _interval, SendCommand, IsConnected) ?? [];
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
        return RconClient?.IsConnected() ?? false;
    }

    public void GetPlayers()
    {
        RconClient?.GetPlayers();
    }

    public void KickPlayer(string id, string reason, string name)
    {
        var connectedPlayer = RconClient?.ConnectedPlayers.Find(x => x.Guid == id);
        if (connectedPlayer != null)
        {
            RconClient?.KickPlayer(connectedPlayer.Id, reason, name);
        }
    }

    public HttpStatusCode BanPlayer(string serverPlayerId, string reason, int duration)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        
        var player = playerRepository?.GetServerPlayer(serverPlayerId);
        
        if (player == null) return HttpStatusCode.NotFound;
        
        return RconClient?.BanPlayer(player.Player.Guid, reason, duration, player.Player.Name) ?? HttpStatusCode.InternalServerError;
    }

    public HttpStatusCode UnbanPlayer(string serverPlayerId)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        
        var player = playerRepository?.GetServerPlayer(serverPlayerId);

        if (player?.Ban == null) return HttpStatusCode.NotFound;
        RconClient?.UnbanPlayer(player.Ban.BanId, player.Player.Name);
        return playerRepository?.RemoveBan(player.Ban.Id) ?? HttpStatusCode.InternalServerError;
    }

    public HttpStatusCode WhitelistPlayer(string serverPlayerId, string name)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var player = playerRepository?.GetServerPlayer(serverPlayerId);
        
        if (player == null) return HttpStatusCode.NotFound;
        
        var instanceService = _serverScope.ServiceProvider.GetService<IInstanceService>();
        var instance = instanceService?.GetInstance(player.InstanceId);
        
        if (instance == null) return HttpStatusCode.NotFound;
        var whitelistedPlayers = GetWhitelistedPlayers(instance.serverFolder);

        playerRepository?.WhitelistPlayer(player.Id);
        if (!whitelistedPlayers.Contains(player.Player.Uid))
        {
            whitelistedPlayers.Add(player.Player.Uid);
        }

        _logger.LogInformation($"{name} was whitelisted");
        return SaveWhitelistedPlayers(instance.serverFolder, whitelistedPlayers);
    }

    public void UnwhitelistPlayer(string serverPlayerId, string name)
    {
        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
        var player = playerRepository?.GetServerPlayer(serverPlayerId);
        
        if (player == null) return;
        
        var instanceService = _serverScope.ServiceProvider.GetService<IInstanceService>();
        var instance = instanceService?.GetInstance(player.InstanceId);
        
        if (instance == null) return;
        
        var whitelistedPlayers = GetWhitelistedPlayers(instance.serverFolder);
        
        playerRepository?.UnWhitelistPlayer(player.Id);
        if (whitelistedPlayers.Contains(player.Player.Uid))
        {
            whitelistedPlayers.Remove(player.Player.Uid);
        }

        SaveWhitelistedPlayers(instance.serverFolder, whitelistedPlayers);
        _logger.LogInformation($"{name} was unwhitelisted");
    }

    public void SendCommand(string command)
    {
        RconClient?.SendCommand(command);
    }

    public void Shutdown()
    {
        RconClient?.Shutdown();
    }

    public void LoadBans()
    {
        if (IsConnected())
        {
            try
            {
                RconClient?.ReloadBans();
                RconClient?.GetBans();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting bans");
            }
        }
    }
}