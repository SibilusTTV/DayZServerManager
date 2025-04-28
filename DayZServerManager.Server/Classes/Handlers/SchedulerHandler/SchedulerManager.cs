using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.VisualBasic.FileIO;

namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class SchedulerManager
    {
        private RCON _rconClient;
        private SchedulerConfig _config;
        private List<JobTimer> _automaticMessages;
        private List<JobTimer> _customMessages;
        private bool _onlyRestarts;
        private int _interval;

        public RCON RconClient { get { return _rconClient; } }

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

            List<string> bannedUsers = new List<string>();
            string bansPath = OperatingSystem.IsWindows() ? Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_BANS_NAME) : Path.Combine(Manager.SERVER_PATH, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_BANS_NAME);
            if (!File.Exists(bansPath))
            {
                File.Create(bansPath);
            }
            else
            {
                using (StreamReader sr = new StreamReader(bansPath))
                {
                    string bansFile = sr.ReadToEnd();
                    bannedUsers = bansFile.Split("\n").ToList();
                }
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

            _rconClient = new RCON(ip, port, password, whitelistedUsers, filteredNicks, bannedUsers, _config);

        }

        public bool Connect()
        {
            Manager.WriteToConsole($"Waiting for {_config.Timeout} seconds until TimeOut is over");
            Thread.Sleep(_config.Timeout * 1000);
            Manager.WriteToConsole("Connecting to the Server");
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
            return _rconClient.IsConnected();
        }

        public int GetPlayers()
        {
            return _rconClient.GetPlayers();
        }

        public void KickPlayer(int id, string reason, string name)
        {
            _rconClient.KickPlayer(id, reason, name);
        }

        public void BanPlayer(int id, string reason, int duration, string name)
        {
            _rconClient.BanPlayer(id, reason, duration, name);
        }

        public void SendCommand(string command)
        {
            _rconClient.SendCommand(command);
        }
    }
}
