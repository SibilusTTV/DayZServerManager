using System.Net;
using Application.IRepository;
using BytexDigital.BattlEye.Rcon;
using BytexDigital.BattlEye.Rcon.Commands;
using BytexDigital.BattlEye.Rcon.Domain;
using BytexDigital.BattlEye.Rcon.Events;
using Domain.Scheduler;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class RconRepository : IRconRepository
{
    private readonly ILogger<RconRepository> _logger;
    private RconClient _client;
    
    public string ChatLog { get; private set;}
    public List<ConnectedPlayer> ConnectedPlayers { get; private set; }
    
    public RconRepository(ILogger<RconRepository> logger)
    {
        _logger = logger;
        ChatLog = "";
        ConnectedPlayers = [];
    }

    public void InitializeRconRepository(string ip, int port, string password)
    {
        _logger.LogInformation($"Creating new RconClient to {ip}:{port} with password {password}");
        _client = new RconClient(ip, port, password);
        _client.ReconnectOnFailure = true;
        _client.MessageReceived += MessageReceived;
        _client.PlayerConnected += PlayerConnected;
        _client.PlayerDisconnected += PlayerDisconnected;
        _client.PlayerRemoved += PlayerRemoved;
    }

    public bool Connect()
    {
        _logger.LogInformation($"Connecting the RconClient");
        var result = _client?.Connect();
        if (result != true)
        {
            Disconnect();
            return false;
        }
        _client?.WaitUntilConnected();
        return result ?? false;
    }

    public void SendCommand(string command)
    {
        try
        {
            _logger.LogInformation($"Sending command {command}");
            _client?.Send(command).WaitUntilResponseReceived();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command");
        }
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting the RconClient");
        _client?.Disconnect();
    }

    public bool IsConnected()
    {
        return _client?.IsConnected ?? false;
    }

    public List<ConnectedPlayer> GetPlayers()
    {
        try
        {
            if (IsConnected())
            {
                _logger.LogInformation("Getting Players");
                List<Player> players = [];
                _client?.Fetch(new GetPlayersRequest(), 5000, out players);

                if (players == null) return [];

                List<ConnectedPlayer> newPlayers = [];
                foreach (var player in players)
                {
                    var index = ConnectedPlayers.FindIndex(x => x.Guid == player.Guid);
                    if (index >= 0)
                    {
                        ConnectedPlayers[index].IsInLobby = player.IsInLobby;
                        ConnectedPlayers[index].Ping = player.Ping;
                    }
                    else
                    {
                        var newPlayer = new ConnectedPlayer(player);
                        ConnectedPlayers.Add(newPlayer);
                        newPlayers.Add(newPlayer);
                    }
                }

                return newPlayers;
            }
            else
            {
                ConnectedPlayers = [];
                return [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting players");
            return [];
        }
    }

    public void KickPlayer(int id, string reason, string name)
    {
        try
        {
            if (IsConnected())
            {
                _client?.Send(new KickCommand(id, reason)).WaitUntilAcknowledged();
                _logger.LogInformation($"The player {name} was kicked for reason \"{reason}\"");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error kicking player");
        }
    }

    public HttpStatusCode BanPlayer(string guid, string reason, int duration, string name)
    {
        try
        {
            if (IsConnected())
            {
                _client?.Send(new BanPlayerCommand(guid, reason, TimeSpan.FromMinutes(duration)))
                    .WaitUntilAcknowledged();
                _logger.LogInformation($"The player {name} was banned for reason \"{reason}\" for {duration} minutes");
                ReloadBans();
                return HttpStatusCode.OK;
            }

            return HttpStatusCode.ServiceUnavailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning player");
            return HttpStatusCode.InternalServerError;
        }
    }

    public HttpStatusCode UnbanPlayer(int banId, string name)
    {
        try
        {
            if (IsConnected())
            {
                _client?.Send(new RemoveBanCommand(banId)).WaitUntilAcknowledged();
                _logger.LogInformation($"The player {name} was unbanned");
                ReloadBans();
                return HttpStatusCode.OK;
            }

            return HttpStatusCode.ServiceUnavailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning player");
            return HttpStatusCode.InternalServerError;
        }
    }

    public void ReloadBans()
    {
        try
        {
            if (IsConnected())
            {
                _logger.LogInformation("Reloading Bans");
                _client?.Send(new LoadBansCommand()).WaitUntilAcknowledged();
                _client?.Send(new SaveBansCommand()).WaitUntilAcknowledged();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading Bans");
        }
    }

    public List<PlayerBan> GetBans()
    {
        try
        {
            if (IsConnected())
            {
                _logger.LogInformation("Getting Bans");
                List<PlayerBan> bans = [];
                _client?.Fetch(new GetBansRequest(), 5000, out bans);
                return bans;
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Bans");
            return [];
        }
    }

    public void Shutdown()
    {
        try
        {
            if (IsConnected())
            {
                _logger.LogInformation("Sending command #shutdown");
                _client?.Send(new ShutdownCommand()).WaitUntilAcknowledged();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shutting down");
        }
    }

    private void MessageReceived(object? state, string message)
    {
        // Add a filter for bad words inside the program and an editor to the UI
        _logger.LogInformation(message);
        ChatLog += $"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}] {message} \n";
    }

    private void PlayerConnected(object? state, PlayerConnectedArgs args)
    {
        _logger.LogInformation("Player {args.Name} connected", args.Name);
        // CreateEditPlayer(args.Guid, args.Name);
    }

    private void PlayerDisconnected(object? state, PlayerDisconnectedArgs args)
    {
        _logger.LogInformation("Player {args.Name} disconnected", args.Name);
        ConnectedPlayers.RemoveAll(x => x.Id == args.Id && x.Name == args.Name);
    }

    private void PlayerRemoved(object? state, PlayerRemovedArgs args)
    {
        _logger.LogInformation("Player {args.Name} was removed", args.Name);
        ConnectedPlayers.RemoveAll(x => x.Guid == args.Guid);
    }
}