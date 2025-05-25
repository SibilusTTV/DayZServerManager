
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Handlers.RestartUpdaterHandler;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses;
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
        private List<string> _whitelistedPlayers;

        public PlayersDB PlayersDB { get { return _playersdb; } }
        public SchedulerConfig Config { get { return _config; } }
        public RCON RconClient { get { return _rconClient; } }
        public List<string> WhitelistedPlayers { get { return _whitelistedPlayers; } }

        public SchedulerManager(string ip, int port, string password, int interval, bool onlyRestarts, List<CustomMessage> customMessages)
        {
            if (!Directory.Exists(Manager.SCHEDULER_PATH))
            {
                Directory.CreateDirectory(Manager.SCHEDULER_PATH);
            }

            _onlyRestarts = onlyRestarts;

            if (interval < 1 && interval > 24)
            {
                throw new Exception("The interval needs to be between 1 and 24");
            }
            else
            {
                _interval = interval;
            }

            if (!File.Exists(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME)))
            {
                _config = new SchedulerConfig();
                JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME), _config);
            }
            else
            {
                SchedulerConfig? tempConfig = JSONSerializer.DeserializeJSONFile<SchedulerConfig>(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME));
                if (tempConfig != null)
                {
                    _config = tempConfig;
                }
                else
                {
                    _config = new SchedulerConfig();
                    JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME), _config);
                }
            }

            _autoLoadBansTimer = new Timer((state) => { LoadBans(); }, null, 10000, 10000);

            if (!File.Exists(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME)))
            {
                _playersdb = new PlayersDB();
                JSONSerializer.SerializeJSONFile<PlayersDB>(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME), _playersdb);
            }
            else
            {
                PlayersDB? players = JSONSerializer.DeserializeJSONFile<PlayersDB>(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME));
                if (players != null)
                {
                    _playersdb = players;
                }
                else
                {
                    _playersdb = new PlayersDB();
                    JSONSerializer.SerializeJSONFile<PlayersDB>(Path.Combine(Manager.SCHEDULER_PATH, Manager.PLAYER_DATABASE_NAME), _playersdb);
                }
            }

            _whitelistedPlayers = new List<string>();
            LoadWhitelistedPlayers();

            _automaticMessages = RestartUpdater.CreateSchedule(false, _onlyRestarts, _interval, this);
            _customMessages = RestartUpdater.CreateCustomJobTimers(_onlyRestarts, _interval, this, customMessages);

            _rconClient = new RCON(ip, port, password, _config, _playersdb);

        }

        public bool Connect()
        {
            Logger.Info($"Waiting for {_config.Timeout} seconds until TimeOut is over");
            Thread.Sleep(_config.Timeout * 1000);
            Logger.Info("Connecting to the Server");
            _rconClient.Connect();
            return true;
        }

        private void LoadWhitelistedPlayers()
        {
            try
            {
                if (File.Exists(Path.Combine(Manager.SERVER_PATH, Manager.WHITELIST_FILE_NAME)))
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(Manager.SERVER_PATH, Manager.WHITELIST_FILE_NAME)))
                    {
                        while (!reader.EndOfStream)
                        {
                            string? line = reader.ReadLine();
                            if (line != null && !string.IsNullOrEmpty(line) && !_whitelistedPlayers.Contains(line))
                            {
                                _whitelistedPlayers.Add(line);
                            }
                        }
                    }
                }
                else
                {
                    if (!Directory.Exists(Manager.SERVER_PATH))
                    {
                        Directory.CreateDirectory(Manager.SERVER_PATH);
                    }
                    using (FileStream writer = File.Create(Path.Combine(Manager.SERVER_PATH, Manager.WHITELIST_FILE_NAME)))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when loading the whitelisted players");
            }
        }

        public void SaveWhitelistedPlayers()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(Manager.SERVER_PATH, Manager.WHITELIST_FILE_NAME)))
                {
                    foreach (string whitelistedPlayer in _whitelistedPlayers)
                    {
                        if (whitelistedPlayer != string.Empty)
                        {
                            writer.WriteLine(whitelistedPlayer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when saving the whitelisted players");
            }
        }

        public void Disconnect()
        {
            if (IsConnected())
            {
                _rconClient.Disconnect();
            }
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

        public void GetPlayers()
        {
            _rconClient.GetPlayers();
        }

        public void KickPlayer(string guid, string reason, string name)
        {
            ConnectedPlayer? connectedPlayer = _rconClient.ConnectedPlayers.Find(x => x.Guid == guid);
            if (connectedPlayer != null)
            {
                _rconClient.KickPlayer(connectedPlayer.Id, reason, name);
            }
        }

        public void BanPlayer(string guid, string reason, int duration, string name)
        {
            ConnectedPlayer? connectedPlayer = _rconClient.ConnectedPlayers.Find(x => x.Guid == guid);
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
            List<BannedPlayer> bannedPlayers = _rconClient.BannedPlayers.FindAll(x => x.Guid == guid);
            foreach (BannedPlayer bannedPlayer in bannedPlayers)
            {
                _rconClient.UnbanPlayer(bannedPlayer.BanId, name);
            }
        }

        public void WhitelistPlayer(string guid, string name)
        {
            LoadWhitelistedPlayers();

            Player? player = _playersdb.Players.Find(player => player.Guid == guid);
            if (player != null)
            {
                if (!_whitelistedPlayers.Contains(player.Uid))
                {
                    _whitelistedPlayers.Add(player.Uid);
                }
            }

            SaveWhitelistedPlayers();
            Logger.Info($"{name} was whitelisted");
        }

        public void UnwhitelistPlayer(string guid, string name)
        {
            LoadWhitelistedPlayers();

            Player? player = _playersdb.Players.Find(player => player.Guid == guid);
            if (player != null)
            {
                if (_whitelistedPlayers.Contains(player.Uid))
                {
                    _whitelistedPlayers.Remove(player.Uid);
                }
            }

            SaveWhitelistedPlayers();
            Logger.Info($"{name} was unwhitelisted");
        }

        public void SendCommand(string command)
        {
            _rconClient.SendCommand(command);
        }

        public void Shutdown()
        {
            _rconClient.Shutdown();
        }

        public void SaveSchedulerConfig(SchedulerConfig config)
        {
            _config = config;
            JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME), _config);
        }

        public void LoadBans()
        {
            if (IsConnected())
            {
                try
                {
                    _rconClient.ReloadBans();
                    _rconClient.GetBans();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error when getting bans");
                }
            }
        }
    }
}
