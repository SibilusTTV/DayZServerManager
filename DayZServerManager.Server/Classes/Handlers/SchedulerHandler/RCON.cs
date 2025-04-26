
using BytexDigital.BattlEye.Rcon;
using BytexDigital.BattlEye.Rcon.Commands;
using BytexDigital.BattlEye.Rcon.Domain;
using BytexDigital.BattlEye.Rcon.Requests;
using BytexDigital.BattlEye.Rcon.Responses;
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using System.Net;
using System.Text.RegularExpressions;

namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class RCON
    {
        private RconClient _client;
        private int _playersCount;
        private List<Player> _players;

        private List<string> whitelistedUsers;
        private List<string> filteredNicks;
        private List<string> bannedUserGuids;

        private SchedulerConfig config;

        public RCON(string ip, int port, string password, List<string> WhitelistedUsers, List<string> FilteredNicks, List<string> BannedUsers, SchedulerConfig Config)
        {
            config = Config;
            _playersCount = 0;
            _players = new List<Player>();
            whitelistedUsers = WhitelistedUsers;
            filteredNicks = FilteredNicks;
            bannedUserGuids = BannedUsers;
            Manager.WriteToConsole($"Creating new RconClient to {ip}:{port} with password {password}");
            _client = new RconClient(ip, port, password);
            _client.PlayerConnected += _client_PlayerConnected;
            _client.PlayerDisconnected += _client_PlayerDisconnected;
            _client.PlayerRemoved += _client_PlayerRemoved;
            _client.MessageReceived += _client_MessageReceived;
            _client.ReconnectOnFailure = true;
        }

        public bool Connect()
        {
            Manager.WriteToConsole($"Connecting the RconClient");
            _client.Connect();
            return _client.WaitUntilConnected(config.ConnectTimeout * 1000);
        }

        public int PlayersCount { get { return _playersCount; } }
        public List<Player> Players { get { return _players; } }

        private void _client_PlayerRemoved(object? sender, BytexDigital.BattlEye.Rcon.Events.PlayerRemovedArgs e)
        {
            Manager.WriteToConsole($"Player {e.Name} was removed");
        }

        private void _client_PlayerDisconnected(object? sender, BytexDigital.BattlEye.Rcon.Events.PlayerDisconnectedArgs e)
        {
            Manager.WriteToConsole($"Player {e.Name} disconnected");
        }

        private void _client_PlayerConnected(object? sender, BytexDigital.BattlEye.Rcon.Events.PlayerConnectedArgs e)
        {
            if (bannedUserGuids.Count > 0 && bannedUserGuids.Contains(e.Guid))
            {
                _client.Send($"kick {e.Id} \"{config.BannedMessage}\"");
                Manager.WriteToConsole($"Player {e.Name} was kicked, because they are banned");
                return;
            }
            else if (config.UseNickFilter && filteredNicks.Count > 0)
            {
                foreach (string filteredNick in filteredNicks)
                {
                    if (!string.IsNullOrEmpty(filteredNick) && e.Name.ToLower().Contains(filteredNick.ToLower()))
                    {
                        _client.Send($"kick {e.Id} \"{config.FilteredNickMessage}\"");
                        Manager.WriteToConsole($"Player {e.Name} was kicked, because they are using forbidden words in their user name");
                        return;
                    }
                }
            }
            else if (config.UseWhiteList && whitelistedUsers.Count > 0)
            {
                bool isWhitelisted = false;
                foreach (string whitelistedUser in whitelistedUsers)
                {
                    if (!string.IsNullOrEmpty(whitelistedUser) && e.Name.ToLower().Contains(whitelistedUser.ToLower()))
                    {
                        isWhitelisted = true;
                    }
                }

                if (!isWhitelisted)
                {
                    _client.Send($"kick {e.Id} \"{config.WhiteListKickMsg}\"");
                    Manager.WriteToConsole($"Player {e.Name} was kicked, because they aren't whitelisted");
                    return;
                }
            }
        }

        private void _client_MessageReceived(object? sender, string e)
        {
            Manager.WriteToConsole(e);

        }

        public void SendCommand(string command)
        {
            _client.Send(command);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public bool IsConnected()
        {
            return _client.IsConnected;
        }

        public int GetPlayers()
        {
            if (_client.IsConnected)
            {
                CommandNetworkRequest request = _client.Send(new GetPlayersRequest());
                bool success = request.WaitUntilResponseReceived(config.ConnectTimeout * 1000);
                if (success)
                {
                    NetworkResponse response = request.Response;
                    string responseString = "";
                    if (response is CommandNetworkResponse)
                    {
                        responseString = ((CommandNetworkResponse)response).Content;
                    }
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        string playerCountRegexPattern = "\\((?'playerCount'[0-9]+) players in total\\)";
                        Regex playerCountRegex = new Regex(playerCountRegexPattern);
                        Match playerCountMatch = playerCountRegex.Match(responseString);
                        if (playerCountMatch.Success)
                        {
                            _playersCount = int.Parse(playerCountMatch.Groups["playerCount"].Value);
                        }

                        string regexPattern = @"(?'id'[0-9]+)[^\S\n]+((?'ip'[0-9]+(?:.[0-9]+)+):(?'port'[0-9]+))[^\S\n]+(?'ping'[0-9]+)[^\S\n]+(?'guid'[0-9a-fA-F]+)\((?'verified'\S+)\)[^\S\n]+(?'name'[^\n]+)";
                        Regex regex = new Regex(regexPattern);
                        MatchCollection matches = regex.Matches(responseString);
                        List<Player> onlinePlayers = new List<Player>();
                        foreach (Match match in matches)
                        {
                            string endString = match.Groups["name"].Value;
                            string name = "";
                            bool isLobby = false;

                            if (endString.EndsWith("(Lobby)"))
                            {
                                name = endString.Substring(0, endString.LastIndexOf("(Lobby)") - 1);
                                isLobby = true;
                            }
                            else
                            {
                                name = endString;
                            }

                            onlinePlayers.Add(new Player(int.Parse(match.Groups["id"].Value),
                                new IPEndPoint(IPAddress.Parse(match.Groups["ip"].Value), int.Parse(match.Groups["port"].Value)),
                                int.Parse(match.Groups["ping"].Value),
                                match.Groups["guid"].Value,
                                name,
                                match.Groups["verified"].Value == "OK",
                                isLobby));
                        }
                        _players = onlinePlayers;
                    }
                }
            }

            return _playersCount;
        }
    }
}
