using System.Net;
using System.Text.RegularExpressions;
using Application.IRepository;
using Application.IService;
using BytexDigital.BattlEye.Rcon;
using BytexDigital.BattlEye.Rcon.Commands;
using Domain.Constants;
using Domain.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class RconService : IRconService
{
    private readonly ILogger<RconService> _logger;
    private readonly IServiceScope _serverScope;
    private RconClient _client;
    private SchedulerConfig? config;
    
    public string ChatLog { get; private set; }
    public int PlayersCount { get; private set; }
    public List<ConnectedPlayer> ConnectedPlayers { get; private set; }
    public RconService(ILogger<RconService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _serverScope = scopeFactory.CreateScope();
        PlayersCount = 0;
        ConnectedPlayers = [];
    }

    public void InitializeRconService(string ip, int port, string password, SchedulerConfig Config)
    {
        config = Config;
        _logger.LogInformation($"Creating new RconClient to {ip}:{port} with password {password}");
        _client = new RconClient(ip, port, password);
        _client.ReconnectOnFailure = true;
        _client.MessageReceived += _client_BattlEyeMessageReceived;
    }

    public bool Connect()
    {
        _logger.LogInformation($"Connecting the RconClient");
        var result = _client?.Connect();
        return result ?? false;
    }

    public void SendCommand(string command)
    {
        _logger.LogInformation($"Sending command {command}");
        _client?.Send(command).WaitUntilResponseReceived();
    }

    public void Disconnect()
    {
        _client?.Disconnect();
    }

    public bool IsConnected()
    {
        return _client?.IsConnected ?? false;
    }

    public void GetPlayers()
    {
        if (IsConnected())
        {
            SendCommand("players");
        }
        else
        {
            ConnectedPlayers.Clear();
        }
    }

    public void KickPlayer(int id, string reason, string name)
    {
        if (IsConnected())
        {
            _client?.Send(new KickCommand(id, reason)).WaitUntilAcknowledged();
            _logger.LogInformation($"The player {name} was kicked for reason \"{reason}\"");
        }
    }

    public void BanPlayer(Guid guid, string reason, int duration, string name)
    {
        if (IsConnected())
        {
            _client?.Send(new BanPlayerCommand(guid.ToString(), reason, TimeSpan.FromMinutes(duration))).WaitUntilAcknowledged();
            _logger.LogInformation($"The player {name} was banned for reason \"{reason}\" for {duration} minutes");
            ReloadBans();
        }
    }

    public void UnbanPlayer(int banId, string name)
    {
        if (IsConnected())
        {
            _client?.Send(new RemoveBanCommand(banId)).WaitUntilAcknowledged();
            _logger.LogInformation($"The player {name} was unbanned");
            ReloadBans();
        }
    }

    public void ReloadBans()
    {
        if (IsConnected())
        {
            _client?.Send(new LoadBansCommand()).WaitUntilAcknowledged();
            _client?.Send(new SaveBansCommand()).WaitUntilAcknowledged();
        }
    }

    public void GetBans()
    {
        if (IsConnected())
        {
            SendCommand("bans");
        }
    }

    public void Shutdown()
    {
        if (IsConnected())
        {
            _logger.LogInformation("Sending command #shutdown");
            _client?.Send(new ShutdownCommand()).WaitUntilAcknowledged();
        }
    }

    private void _client_BattlEyeMessageReceived(object? state, string message)
    {
        if (message.Contains("GUID Bans"))
        {
            RecievedBans(message);
        }
        else if (message.Contains("Players on server:"))
        {
            RecievedPlayers(message);
        }
        else if (message.Contains("BE GUID"))
        {
            RecievedPlayerConnected(message);

            // Add a filter for bad words inside the program and an editor to the UI
            _logger.LogInformation(message);
            ChatLog += $"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}] {message} \n";
        }
        else
        {
            // Add a filter for bad words inside the program and an editor to the UI
            _logger.LogInformation(message);
            ChatLog += $"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}] {message} \n";
        }
    }

    private void RecievedBans(string message)
    {
        var pattern = @"(?'banid'[0-9]+)[^\S\n]+(?'guid'[0-9A-Fa-f]+)[^\S\n]+(?'remainingTime'[0-9]+)[^\S\n]+\""(?'reason'[^\n]*)\""";
        var regex = new Regex(pattern);
        var matches = regex.Matches(message);

        var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();

        if (matches.Count <= 0)
        {
            playerRepository?.ClearBans();
            return;
        }

        foreach (Match match in matches)
        {
            var banId = int.Parse(match.Groups["banid"].Value);
            var guid = match.Groups["guid"].Value;
            var remainingTime = int.Parse(match.Groups["remainingTime"].Value);
            var reason = match.Groups["reason"].Value;
            
            var bannedPlayer = playerRepository?.GetBannedServerPlayer(banId, Guid.Parse(guid), reason);

            if (bannedPlayer == null && remainingTime > 0)
            {
                playerRepository?.CreateNewBan(banId, Guid.Parse(guid), remainingTime, reason);
            }
            else if (bannedPlayer != null && bannedPlayer.Ban != null)
            {
                if (remainingTime <= 0)
                {
                    playerRepository?.RemoveBan(bannedPlayer.Ban.Id);
                }
                else
                {
                    playerRepository?.UpdateRemainingTime(bannedPlayer.Ban.Id, remainingTime);
                }
            }
        }
    }

    private void RecievedPlayers(string message)
    {
        var playerCountRegexPattern = "\\((?'playerCount'[0-9]+) players in total\\)";
        var playerCountRegex = new Regex(playerCountRegexPattern);
        var playerCountMatch = playerCountRegex.Match(message);
        if (playerCountMatch.Success)
        {
            PlayersCount = int.Parse(playerCountMatch.Groups["playerCount"].Value);
        }

        var regexPattern = @"(?'id'[0-9]+)[^\S\n]+((?'ip'[0-9]+(?:.[0-9]+)+):(?'port'[0-9]+))[^\S\n]+(?'ping'[0-9]+)[^\S\n]+(?'guid'[0-9a-fA-F]+)\((?'verified'\S+)\)[^\S\n]+(?'name'[^\n]+)";
        var regex = new Regex(regexPattern);
        var matches = regex.Matches(message);
        var onlinePlayers = new List<ConnectedPlayer>();
        foreach (Match match in matches)
        {
            var endString = match.Groups["name"].Value;
            var name = "";
            var isInLobby = false;

            if (endString.EndsWith("(Lobby)"))
            {
                name = endString.Substring(0, endString.LastIndexOf("(Lobby)") - 1);
                isInLobby = true;
            }
            else
            {
                name = endString;
            }

            var guid = match.Groups["guid"].Value;
            var id = int.Parse(match.Groups["id"].Value);
            var ping = int.Parse(match.Groups["ping"].Value);
            var isVerified = match.Groups["verified"].Value == "OK";
            var ip = match.Groups["ip"].Value + ":" + match.Groups["port"].Value;
            
            onlinePlayers.Add(new ConnectedPlayer(name, guid, id, ping, isVerified, isInLobby, ip));
            
            var playerRepository = _serverScope.ServiceProvider.GetService<IPlayerRepository>();
            
            var player = playerRepository?.GetPlayer(Guid.Parse(guid));
            if (player == null)
            {
                playerRepository?.CreateEditPlayer(new Player(Guid.Parse(guid), name, "", isVerified, ip));
            }
        }
        ConnectedPlayers = onlinePlayers;
        PlayersCount = onlinePlayers.Count;
    }

    private void RecievedPlayerConnected(string message)
    {
        var pattern = @"Player #(?'id'[0-9]+) (?'name'[^\n]+) - BE GUID: (?'guid'[A-Fa-f0-9]+)";
        var regex = new Regex(pattern);
        var match = regex.Match(message);

        if (match.Success)
        {
            var id = match.Groups["id"].Value;
            var name = match.Groups["name"].Value;
            var guid = match.Groups["guid"].Value;

            // Add a bad words list editor to the UI
            if ((config?.UseNickFilter ?? false) && config?.BadNames.Count > 0)
            {
                foreach (var badName in config.BadNames)
                {
                    if (string.IsNullOrEmpty(badName) ||
                        !name.Contains(badName, StringComparison.CurrentCultureIgnoreCase)) continue;
                    
                    SendCommand($"kick {id} \"{config.FilteredNickMsg}\"");
                    _logger.LogInformation($"Player {name} was kicked, because they are using forbidden words in their user name");
                    return;
                }
            }
        }

        GetPlayers();
    }
}