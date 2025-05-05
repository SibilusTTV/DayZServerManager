
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses.PlayersDB;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class SchedulerManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private RCON _rconClient;
        private SchedulerConfig _config;
        private Timer? _autoLoadBansTimer;
        private List<JobTimer> _automaticMessages;
        private List<JobTimer> _customMessages;
        private PlayersDB _playersdb;
        private bool _onlyRestarts;
        private int _interval;
        private List<BannedPlayer> _bannedPlayers;

        public List<BannedPlayer> BannedPlayers { get { return _bannedPlayers; } }
        public PlayersDB PlayersDB { get { return _playersdb; } }
        public SchedulerConfig Config { get { return _config; } }

        public SchedulerManager(string ip, int port, string password, int interval, bool onlyRestarts, List<CustomMessage> customMessages)
        {
            _onlyRestarts = onlyRestarts;

            if (interval < 1 && interval > 24)
            {
                throw new Exception("The interval needs to be between 1 and 24");
            }
            else
            {
                _interval = interval;
            }

            SchedulerConfig? tempConfig = JSONSerializer.DeserializeJSONFile<SchedulerConfig>(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME));
            if (tempConfig == null)
            {
                _config = new SchedulerConfig();
                JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME), _config);
            }
            else
            {
                _config = tempConfig;
            }

            _autoLoadBansTimer = new Timer((state) => { LoadBans(); }, null, 10000, 10000);

            PlayersDB? players = JSONSerializer.DeserializeJSONFile<PlayersDB>(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME));
            if (players == null)
            {
                _playersdb = new PlayersDB();
                JSONSerializer.SerializeJSONFile<PlayersDB>(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME), _playersdb);
            }
            else
            {
                _playersdb = players;
            }

            List<string> whitelistedUsers = new List<string>();
            if (_config.UseWhiteList)
            {
                if (!File.Exists(Path.Combine(Manager.SCHEDULER_PATH, _config.WhiteListFile)))
                {
                    File.Create(Path.Combine(Manager.SCHEDULER_PATH, _config.WhiteListFile));
                }
                else
                {
                    using (StreamReader sr = new StreamReader(Path.Combine(Manager.SCHEDULER_PATH, _config.WhiteListFile)))
                    {
                        string whitelistFile = sr.ReadToEnd();
                        whitelistedUsers = whitelistFile.Split("\n").ToList();
                    }
                }
            }

            List<string> filteredNicks = new List<string>();
            if (_config.UseNickFilter)
            {
                if (!File.Exists(Path.Combine(Manager.SCHEDULER_PATH, _config.NickFilterFile)))
                {
                    File.Create(Path.Combine(Manager.SCHEDULER_PATH, _config.NickFilterFile));
                }
                else
                {
                    using (StreamReader sr = new StreamReader(Path.Combine(Manager.SCHEDULER_PATH, _config.NickFilterFile)))
                    {
                        string nickFilteFile = sr.ReadToEnd();
                        filteredNicks = nickFilteFile.Split("\n").ToList();
                    }
                }
            }

            _automaticMessages = RestartUpdater.CreateSchedule(false, _onlyRestarts, _interval, this);
            _customMessages = RestartUpdater.CreateCustomJobTimers(_onlyRestarts, _interval, this, customMessages);

            _bannedPlayers = LoadBans();

            _rconClient = new RCON(ip, port, password, whitelistedUsers, filteredNicks, _config, _playersdb);

        }

        public bool Connect()
        {
            Logger.Info($"Waiting for {_config.Timeout} seconds until TimeOut is over");
            Thread.Sleep(_config.Timeout * 1000);
            Logger.Info("Connecting to the Server");
            return _rconClient.Connect();
        }

        public void ChangeToNormalMode()
        {
            KillAutomaticTasks();
            _automaticMessages = RestartUpdater.CreateSchedule(false, _onlyRestarts, _interval, this);
        }

        public void ChangeToUpdateMode()
        {
            KillAutomaticTasks();
            _automaticMessages = RestartUpdater.CreateSchedule(true, _onlyRestarts, _interval, this);
        }

        public void KillAutomaticTasks()
        {
            foreach (JobTimer timer in _automaticMessages)
            {
                timer.Dispose();
            }
            _automaticMessages.Clear();
        }

        public void KillCustomTasks()
        {
            foreach (JobTimer timer in _customMessages)
            {
                timer.Dispose();
            }
            _customMessages.Clear();
        }

        public bool IsConnected()
        {
            return _rconClient != null ? _rconClient.IsConnected() : false;
        }

        public int GetPlayers()
        {
            int players = _rconClient.GetPlayers();
            Task t = new Task(() => { JSONSerializer.SerializeJSONFile<PlayersDB>(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME), _playersdb); });
            t.Start();
            return players;
        }

        public void KickPlayer(string guid, string reason, string name)
        {
            ConnectedPlayer? connectedPlayer = Manager.props.players.Find(x => x.Guid == guid);
            if (connectedPlayer != null)
            {
                _rconClient.KickPlayer(connectedPlayer.Id, reason, name);
            }
        }

        public void BanPlayer(string guid, string reason, int duration, string name)
        {
            ConnectedPlayer? connectedPlayer = Manager.props.players.Find(x => x.Guid == guid);
            if (connectedPlayer != null)
            {
                _rconClient.BanOnlinePlayer(connectedPlayer.Id, reason, duration, name);
            }
            else
            {
                _rconClient.BanOfflinePlayer(guid, reason, duration, name);
            }
        }

        public void UnbanPlayer(string guid, string name)
        {
            List<BannedPlayer> bannedPlayers = _bannedPlayers.FindAll(x => x.Guid == guid);
            foreach (BannedPlayer bannedPlayer in bannedPlayers)
            {
                _rconClient.UnbanPlayer(bannedPlayer.BanId, name);
            }
        }

        public void SendCommand(string command)
        {
            _rconClient.SendCommand(command);
        }

        public void SaveSchedulerConfig(SchedulerConfig config)
        {
            _config = config;
            JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME), _config);
        }

        public List<BannedPlayer> LoadBans()
        {
            if (_bannedPlayers == null)
            {
                _bannedPlayers = new List<BannedPlayer>();
            }

            if (!IsConnected())
            {
                return _bannedPlayers;
            }

            try
            {
                _rconClient.ReloadBans();
                string bans = _rconClient.GetBans();
                string pattern = @"(?'banid'[0-9]+)[^\S\n]+(?'guid'[0-9A-Fa-f]+)[^\S\n]+(?'remainingTime'[0-9]+)[^\S\n]+\""(?'reason'[^\n]*)\""";
                Regex regex = new Regex(pattern);
                MatchCollection matches = regex.Matches(bans);

                List<BannedPlayer> bannedPlayers = new List<BannedPlayer>();

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

                return _bannedPlayers;
            }
            catch (Exception ex)
            {
                Logger.Error("Error when getting bans", ex);
                return _bannedPlayers;
            }
        }
    }
}
