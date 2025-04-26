using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.VisualBasic.FileIO;

namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class SchedulerManager
    {
        private RCON rconClient;
        private SchedulerConfig config;
        private SchedulerFile scheduler;
        private List<Timer> tasks;

        private bool isOnUpdate;
        private bool onlyRestarts;
        private int interval;

        private static string BAN_PATH = Path.Combine(Manager.SERVER_PATH, Manager.BAN_FILE_NAME);

        public RCON RconClient { get { return rconClient; } }

        public SchedulerManager(string ip, int port, string password, bool isOnUpdate, bool onlyRestarts, int interval, List<JobItem> jobItems)
        {
            this.isOnUpdate = isOnUpdate;
            this.onlyRestarts = onlyRestarts;

            if (interval <= 0)
            {
                throw new Exception("The interval needs to be at least 1");
            }
            else
            {
                this.interval = interval;
            }

            SchedulerConfig? tempConfig = JSONSerializer.DeserializeJSONFile<SchedulerConfig>(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME));
            if (tempConfig == null)
            {
                config = new SchedulerConfig();
                JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_NAME), config);
            }
            else
            {
                config = tempConfig;
            }

            List<string> bannedUsers = new List<string>();
            if (!File.Exists(BAN_PATH))
            {
                File.Create(BAN_PATH);
            }
            else
            {
                using (StreamReader sr = new StreamReader(BAN_PATH))
                {
                    string bansFile = sr.ReadToEnd();
                    bannedUsers = bansFile.Split("\n").ToList();
                }
            }

            List<string> whitelistedUsers = new List<string>();
            if (config.UseWhiteList)
            {
                if (!File.Exists(Path.Combine(Manager.SCHEDULER_PATH, config.WhiteListFile)))
                {
                    File.Create(Path.Combine(Manager.SCHEDULER_PATH, config.WhiteListFile));
                }
                else
                {
                    using (StreamReader sr = new StreamReader(Path.Combine(Manager.SCHEDULER_PATH, config.WhiteListFile)))
                    {
                        string whitelistFile = sr.ReadToEnd();
                        whitelistedUsers = whitelistFile.Split("\n").ToList();
                    }
                }
            }

            List<string> filteredNicks = new List<string>();
            if (config.UseNickFilter)
            {
                if (!File.Exists(Path.Combine(Manager.SCHEDULER_PATH, config.NickFilterFile)))
                {
                    File.Create(Path.Combine(Manager.SCHEDULER_PATH, config.NickFilterFile));
                }
                else
                {
                    using (StreamReader sr = new StreamReader(Path.Combine(Manager.SCHEDULER_PATH, config.NickFilterFile)))
                    {
                        string nickFilteFile = sr.ReadToEnd();
                        filteredNicks = nickFilteFile.Split("\n").ToList();
                    }
                }
            }

            SchedulerFile? tempScheduler = RestartUpdater.CreateNewSchedulerFile(isOnUpdate, onlyRestarts, interval, jobItems);
            if (tempScheduler == null)
            {
                throw new Exception("It's not a good time to update");
            }
            else
            {
                scheduler = tempScheduler;
            }

            this.tasks = CreateTasks(scheduler.JobItems);

            rconClient = new RCON(ip, port, password, whitelistedUsers, filteredNicks, bannedUsers, config);

        }

        public bool Connect()
        {
            Manager.WriteToConsole($"Waiting for {config.Timeout} seconds until TimeOut is over");
            Thread.Sleep(config.Timeout * 1000);
            Manager.WriteToConsole("Connecting to the Server");
            return rconClient.Connect();

        }

        private List<Timer> CreateTasks(List<JobItem> items)
        {
            List<Timer> tempTasks = new List<Timer>();
            foreach (JobItem item in items)
            {
                DateTime now = DateTime.Now;
                DateTime scheduledJob;
                if (item.IsTimeOfDay)
                {
                    scheduledJob = DateTime.Today;
                }
                else
                {
                    scheduledJob = DateTime.Now;
                }
                scheduledJob = scheduledJob.AddHours(item.WaitTime["hours"]).AddMinutes(item.WaitTime["minutes"]).AddSeconds(item.WaitTime["seconds"]);

                TimeSpan waitTime = scheduledJob - now;
                if (waitTime.TotalSeconds < 0)
                {
                    scheduledJob = scheduledJob.AddDays(1);
                    waitTime = scheduledJob - now;
                }

                TimeSpan sp;
                if (item.Interval["hours"] == 0 && item.Interval["minutes"] == 0 && item.Interval["seconds"] == 0)
                {
                    sp = TimeSpan.FromDays(1);
                }
                else
                {
                    sp = TimeSpan.FromHours(item.Interval["hours"]);
                    sp.Add(TimeSpan.FromMinutes(item.Interval["minutes"]));
                    sp.Add(TimeSpan.FromSeconds(item.Interval["seconds"]));
                }

                if (item.Loop > 0)
                {
                    for (int i = 0; i < item.Loop; i++)
                    {
                        tempTasks.Add(new Timer((state) => { ExecuteFunction(item); }, null, waitTime.Add(TimeSpan.FromSeconds(i)), sp));
                    }
                }
                else
                {
                    tempTasks.Add(new Timer((state) => { ExecuteFunction(item); }, null, waitTime, sp));
                }
            }
            Manager.WriteToConsole($"{tempTasks.Count} tasks were created");
            return tempTasks;
        }

        private void ExecuteFunction(JobItem item)
        {
            Manager.WriteToConsole($"Sending command {item.Cmd}");
            if (rconClient.IsConnected())
            {
                rconClient.SendCommand(item.Cmd);
            }
        }

        public void KillTasks()
        {
            if (tasks != null)
            {
                foreach (Timer timer in tasks)
                {
                    if (timer != null)
                    {
                        timer.Dispose();
                    }
                }
                tasks.Clear();
            }
        }

        public bool IsConnected()
        {
            return rconClient.IsConnected();
        }

        public int GetPlayers()
        {
            return rconClient.GetPlayers();
        }

        public void KickPlayer(int id, string reason, string name)
        {
            rconClient.KickPlayer(id, reason, name);
        }

        public void BanPlayer(int id, string reason, int duration, string name)
        {
            rconClient.BanPlayer(id, reason, duration, name);
        }
    }
}
