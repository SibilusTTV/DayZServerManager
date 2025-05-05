
using BytexDigital.BattlEye.Rcon;
using BytexDigital.BattlEye.Rcon.Commands;
using BytexDigital.BattlEye.Rcon.Requests;
using BytexDigital.BattlEye.Rcon.Responses;
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses.PlayersDB;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class RCON
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private RconClient _client;
        private int _playersCount;
        private List<ConnectedPlayer> _players;
        private PlayersDB _playersDB;

        private List<string> whitelistedUsers;
        private List<string> filteredNicks;

        private SchedulerConfig config;

        public RCON(string ip, int port, string password, List<string> WhitelistedUsers, List<string> FilteredNicks, SchedulerConfig Config, PlayersDB playersDB)
        {
            _playersDB = playersDB;
            config = Config;
            _playersCount = 0;
            _players = new List<ConnectedPlayer>();
            whitelistedUsers = WhitelistedUsers;
            filteredNicks = FilteredNicks;
            Logger.Info($"Creating new RconClient to {ip}:{port} with password {password}");
            _client = new RconClient(ip, port, password);
            //_client.PlayerDisconnected += _client_PlayerDisconnected;
            //_client.PlayerRemoved += _client_PlayerRemoved;
            _client.MessageReceived += _client_MessageReceived;
            _client.PlayerConnected += _client_PlayerConnected;
            _client.ReconnectOnFailure = true;
        }

        public bool Connect()
        {
            Logger.Info($"Connecting the RconClient");
            _client.Connect();
            return _client.WaitUntilConnected(config.ConnectTimeout * 1000);
        }

        public int PlayersCount { get { return _playersCount; } }
        public List<ConnectedPlayer> Players { get { return _players; } }

        //private void _client_PlayerRemoved(object? sender, BytexDigital.BattlEye.Rcon.Events.PlayerRemovedArgs e)
        //{
        //    Manager.WriteToConsole($"Player {e.Name} was removed");
        //}

        //private void _client_PlayerDisconnected(object? sender, BytexDigital.BattlEye.Rcon.Events.PlayerDisconnectedArgs e)
        //{
        //    Manager.WriteToConsole($"Player {e.Name} disconnected");
        //}

        private void _client_PlayerConnected(object? sender, BytexDigital.BattlEye.Rcon.Events.PlayerConnectedArgs e)
        {
            // Add a bad words list editor to the UI
            if (config.UseNickFilter && filteredNicks.Count > 0)
            {
                foreach (string filteredNick in filteredNicks)
                {
                    if (!string.IsNullOrEmpty(filteredNick) && e.Name.ToLower().Contains(filteredNick.ToLower()))
                    {
                        _client.Send($"kick {e.Id} \"{config.FilteredNickMsg}\"");
                        Logger.Info($"Player {e.Name} was kicked, because they are using forbidden words in their user name");
                        return;
                    }
                }
            }
            else if (config.UseWhiteList && whitelistedUsers.Count > 0)
            {
                // Add a whitelist field and button to the Playerdatabase
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
                    Logger.Info($"Player {e.Name} was kicked, because they aren't whitelisted");
                    return;
                }
            }

            GetPlayers();
        }

        private void _client_MessageReceived(object? sender, string e)
        {
            // Add a filter for bad words inside the program and an editor to the UI
            // Add a Chat log to the UI
            Logger.Info(e);
            Manager.props.chatLog += e + "\n";
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
                        List<ConnectedPlayer> onlinePlayers = new List<ConnectedPlayer>();
                        foreach (Match match in matches)
                        {
                            string endString = match.Groups["name"].Value;
                            string name = "";
                            bool isInLobby = false;

                            if (endString.EndsWith("(Lobby)"))
                            {
                                name = endString.Substring(0, endString.LastIndexOf("(Lobby)") - 1);
                                isInLobby = true;
                            }
                            else
                            {
                                name = endString;
                            }

                            string guid = match.Groups["guid"].Value;
                            int id = int.Parse(match.Groups["id"].Value);
                            int ping = int.Parse(match.Groups["ping"].Value);
                            bool isVerified = match.Groups["verified"].Value == "OK";
                            string ip = match.Groups["ip"].Value + ":" + match.Groups["port"].Value;

                            onlinePlayers.Add(new ConnectedPlayer(name, guid, id, ping, isVerified, isInLobby, ip));

                            Player? player = _playersDB.Players.Find(x => x.Guid == guid);
                            if (player == null)
                            {
                                _playersDB.Players.Add(new Player(name, guid, isVerified, ip));
                            }
                        }
                        _players = onlinePlayers;
                    }
                }
            }

            Manager.props.players = _players;
            Manager.props.playersCount = _players.Count;

            return _playersCount;
        }

        public void KickPlayer(int id, string reason, string name)
        {
            if (_client.IsConnected)
            {
                CommandNetworkRequest request = _client.Send($"kick {id} \"{reason}\"");
                request.WaitUntilAcknowledged(config.ConnectTimeout * 1000);
                Logger.Info($"The player {name} was kicked for reason \"{reason}\"");
            }
        }

        public void BanOnlinePlayer(int id, string reason, int duration, string name)
        {
            if (_client.IsConnected)
            {
                CommandNetworkRequest request = _client.Send($"ban {id} {duration} \"{reason}\"");
                request.WaitUntilAcknowledged(config.ConnectTimeout * 1000);
                Logger.Info($"The player {name} was banned for reason \"{reason}\"");
            }
        }

        public void BanOfflinePlayer(string guid, string reason, int duration, string name)
        {
            if (_client.IsConnected)
            {
                CommandNetworkRequest request = _client.Send($"addBan {guid} {duration} \"{reason}\"");
                request.WaitUntilAcknowledged(config.ConnectTimeout * 1000);
                Logger.Info($"The player {name} was banned for reason \"{reason}\"");
            }
        }

        public void UnbanPlayer(int banId, string name)
        {
            if (_client.IsConnected)
            {
                CommandNetworkRequest request = _client.Send($"removeBan {banId}");
                request.WaitUntilAcknowledged(config.ConnectTimeout * 1000);
                Logger.Info($"The player {name} was unbanned");
                ReloadBans();
            }
        }

        public void ReloadBans()
        {
            if (_client.IsConnected)
            {
                CommandNetworkRequest reloadBansRequest = _client.Send("loadBans");
                reloadBansRequest.WaitUntilAcknowledged(config.ConnectTimeout * 1000);
            }
        }

        public string GetBans()
        {
            if (_client.IsConnected)
            {
                CommandNetworkRequest getBansRequest = _client.Send("bans");
                getBansRequest.WaitUntilResponseReceived(config.ConnectTimeout * 1000);
                if (getBansRequest.Response != null && getBansRequest.Response is CommandNetworkResponse)
                {
                    return ((CommandNetworkResponse)getBansRequest.Response).Content;
                }
            }
            return string.Empty;
        }
    }
}
