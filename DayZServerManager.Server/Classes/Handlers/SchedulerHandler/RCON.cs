
using BattleNET;
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses.PlayersDB;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class RCON
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private BattlEyeClient _client;
        private int _playersCount;
        private List<ConnectedPlayer> _players;
        private List<BannedPlayer> _bannedPlayers;
        private PlayersDB _playersDB;

        private SchedulerConfig config;

        public RCON(string ip, int port, string password, SchedulerConfig Config, PlayersDB playersDB)
        {
            _playersDB = playersDB;
            config = Config;
            _playersCount = 0;
            _players = new List<ConnectedPlayer>();
            _bannedPlayers = new List<BannedPlayer>();
            Logger.Info($"Creating new RconClient to {ip}:{port} with password {password}");
            _client = new BattlEyeClient(new BattlEyeLoginCredentials(IPAddress.Parse(ip), port, password));
            _client.ReconnectOnPacketLoss = true;
            _client.BattlEyeMessageReceived += _client_BattlEyeMessageReceived;
        }

        public BattlEyeConnectionResult Connect()
        {
            Logger.Info($"Connecting the RconClient");
            return _client.Connect();
        }

        public int PlayersCount { get { return _playersCount; } }
        public List<ConnectedPlayer> ConnectedPlayers { get { return _players; } }
        public List<BannedPlayer> BannedPlayers { get { return _bannedPlayers; } }

        public int SendCommand(string command)
        {
            return _client.SendCommand(command);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public bool IsConnected()
        {
            return _client.Connected;
        }

        public void GetPlayers()
        {
            if (IsConnected())
            {
                SendCommand("players");
            }
        }

        public void KickPlayer(int id, string reason, string name)
        {
            if (IsConnected())
            {
                //SendCommand($"kick {id} \"{reason}\"");
                _client.SendCommand(BattlEyeCommand.Kick, $"{id} \"{reason}\"");
                Logger.Info($"The player {name} was kicked for reason \"{reason}\"");
            }
        }

        public void BanOnlinePlayer(int id, string reason, int duration, string name)
        {
            if (IsConnected())
            {
                //SendCommand($"ban {id} {duration} \"{reason}\"");
                _client.SendCommand(BattlEyeCommand.Ban, $"{id} {duration} \"{reason}\"");
                Logger.Info($"The player {name} was banned for reason \"{reason}\" for {duration} minutes");
            }
        }

        public void BanOfflinePlayer(string guid, string reason, int duration, string name)
        {
            if (IsConnected())
            {
                //SendCommand($"addBan {guid} {duration} \"{reason}\"");
                _client.SendCommand(BattlEyeCommand.AddBan, $"{guid} {duration} \"{reason}\"");
                Logger.Info($"The player {name} was banned for reason \"{reason}\" for {duration} minutes");
                ReloadBans();
            }
        }

        public void UnbanPlayer(int banId, string name)
        {
            if (IsConnected())
            {
                //SendCommand($"removeBan {banId}");
                _client.SendCommand(BattlEyeCommand.RemoveBan, banId.ToString());
                Logger.Info($"The player {name} was unbanned");
                ReloadBans();
            }
        }

        public void ReloadBans()
        {
            if (IsConnected())
            {
                _client.SendCommand(BattlEyeCommand.LoadBans);
                _client.SendCommand(BattlEyeCommand.WriteBans);
                //SendCommand("loadBans");
                //SendCommand("writeBans");
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
                _client.SendCommand(BattlEyeCommand.Shutdown);
            }
        }

        private void _client_BattlEyeMessageReceived(BattlEyeMessageEventArgs args)
        {
            if (args.Message.Contains("GUID Bans"))
            {
                RecievedBans(args.Message);
            }
            else if (args.Message.Contains("Players on server:"))
            {
                RecievedPlayers(args.Message);
            }
            else if (args.Message.Contains("BE GUID"))
            {
                RecievedPlayerConnected(args.Message);

                // Add a filter for bad words inside the program and an editor to the UI
                Logger.Info(args.Message);
                Manager.props.chatLog += $"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}] {args.Message} \n";
            }
            else
            {
                // Add a filter for bad words inside the program and an editor to the UI
                Logger.Info(args.Message);
                Manager.props.chatLog += $"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}] {args.Message} \n";
            }
        }

        private void RecievedBans(string message)
        {
            string pattern = @"(?'banid'[0-9]+)[^\S\n]+(?'guid'[0-9A-Fa-f]+)[^\S\n]+(?'remainingTime'[0-9]+)[^\S\n]+\""(?'reason'[^\n]*)\""";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(message);

            if (matches.Count <= 0)
            {
                _bannedPlayers = new List<BannedPlayer>();
                return;
            }

            foreach (Match match in matches)
            {
                int banId = int.Parse(match.Groups["banid"].Value);
                string guid = match.Groups["guid"].Value;
                int remainingTime = int.Parse(match.Groups["remainingTime"].Value);
                string reason = match.Groups["reason"].Value;

                BannedPlayer? bannedPlayer = _bannedPlayers.Find(x => x.BanId == banId && x.Guid == guid && x.Reason == reason);
                if (bannedPlayer == null && remainingTime > 0)
                {
                    _bannedPlayers.Add(new BannedPlayer(banId, guid, remainingTime, reason));
                }
                else if (bannedPlayer != null)
                {
                    if (remainingTime <= 0)
                    {
                        _bannedPlayers.Remove(bannedPlayer);
                    }
                    else
                    {
                        bannedPlayer.RemainingTime = remainingTime;
                    }
                }
            }
        }

        private void RecievedPlayers(string message)
        {
            string playerCountRegexPattern = "\\((?'playerCount'[0-9]+) players in total\\)";
            Regex playerCountRegex = new Regex(playerCountRegexPattern);
            Match playerCountMatch = playerCountRegex.Match(message);
            if (playerCountMatch.Success)
            {
                _playersCount = int.Parse(playerCountMatch.Groups["playerCount"].Value);
            }

            string regexPattern = @"(?'id'[0-9]+)[^\S\n]+((?'ip'[0-9]+(?:.[0-9]+)+):(?'port'[0-9]+))[^\S\n]+(?'ping'[0-9]+)[^\S\n]+(?'guid'[0-9a-fA-F]+)\((?'verified'\S+)\)[^\S\n]+(?'name'[^\n]+)";
            Regex regex = new Regex(regexPattern);
            MatchCollection matches = regex.Matches(message);
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
                    _playersDB.Players.Add(new Player(name, guid, "", isVerified, ip));
                }
            }
            _players = onlinePlayers;

            Manager.props.players = _players;
            Manager.props.playersCount = _players.Count;

            try
            {
                JSONSerializer.SerializeJSONFile<PlayersDB>(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME), _playersDB);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when saving the playersDB");
            }
        }

        private void RecievedPlayerConnected(string message)
        {
            string pattern = @"Player #(?'id'[0-9]+) (?'name'[^\n]+) - BE GUID: (?'guid'[A-Fa-f0-9]+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(message);

            if (match.Success)
            {
                string id = match.Groups["id"].Value;
                string name = match.Groups["name"].Value;
                string guid = match.Groups["guid"].Value;

                // Add a bad words list editor to the UI
                if (config.UseNickFilter && config.BadNames.Count > 0)
                {
                    foreach (string badName in config.BadNames)
                    {
                        if (!string.IsNullOrEmpty(badName) && name.ToLower().Contains(badName.ToLower()))
                        {
                            SendCommand($"kick {id} \"{config.FilteredNickMsg}\"");
                            Logger.Info($"Player {name} was kicked, because they are using forbidden words in their user name");
                            return;
                        }
                    }
                }
            }

            GetPlayers();
        }
    }
}
