using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Handlers.SchedulerHandler;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;

namespace DayZServerManager.Server.Classes.Handlers.RestartUpdaterHandler
{
    public static class RestartUpdater
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #region Constants
        private const string CMD_SHUTDOWN = "#shutdown";
        private const string CMD_LOCK = "#lock";
        private const string CMD_ONEHOUR = "say -1 Alert: The Server is restarting in 1 hour";
        private const string CMD_THIRTYMINUTES = "say -1 Alert: The Server is restarting in 30 minutes";
        private const string CMD_FIFTEENMINUTES = "say -1 Alert: The Server is restarting in 15 minutes";
        private const string CMD_FIVEMINUTES = "say -1 Alert: The Server is restarting in 5 minutes! Please land your helicopters as soon as possible!";
        private const string CMD_ONEMINUTE = "say -1 Alert: The Server is restarting in 1 minute! Please log out in order to prevent inventory loss!";
        private const string CMD_RESTARTINGNOW = "say -1 Alert: The Server is restarting now!!";

        private const string CMD_UPDATE_TWENTYMINUTES = "say -1 Alert: The Server is restarting in 20 minutes to load updated mods! Please restart your game afterwards!";
        private const string CMD_UPDATE_FIFTEENMINUTES = "say -1 Alert: The Server is restarting in 15 minutes to load updated mods! Please restart your game afterwards!";
        private const string CMD_UPDATE_TENMINUTES = "say -1 Alert: The Server is restarting in 10 minutes to load updated mods! Please restart your game afterwards!";
        private const string CMD_UPDATE_FIVEMINUTES = "say -1 Alert: The Server is restarting in 5 minutes to load updated mods! ! Please land your helicopters as soon as possible and restart your game afterwards!";
        private const string CMD_UPDATE_ONEMINUTE = "say -1 Alert: The Server is restarting in 1 minute to load updated mods! ! Please log out in order to prevent inventory loss and restart your game afterwards!";
        private const string CMD_UPDATE_RESTARTINGNOW = "say -1 Alert: The Server is restarting now to load updated mods!! Please your restart your game afterwards!";

        private readonly static TimeSpan OneHourTimeSpan = new(1, 0, 0);
        private readonly static TimeSpan ThirtyMinuteTimeSpan = new(0, 30, 0);
        private readonly static TimeSpan TwentyMinuteTimeSpan = new(0, 20, 0);
        private readonly static TimeSpan FifteenMinuteTimeSpan = new(0, 15, 0);
        private readonly static TimeSpan TenMinuteTimeSpan = new(0, 10, 0);
        private readonly static TimeSpan FiveMinuteTimeSpan = new(0, 5, 0);
        private readonly static TimeSpan OneMinuteTimeSpan = new(0, 1, 0);
        private readonly static TimeSpan TenSecondTimeSpan = new(0, 0, 10);
        private readonly static TimeSpan NineSecondTimeSpan = new(0, 0, 9);
        private readonly static TimeSpan EightSecondTimeSpan = new(0, 0, 8);
        private readonly static TimeSpan SevenSecondTimeSpan = new(0, 0, 7);
        private readonly static TimeSpan SixSecondTimeSpan = new(0, 0, 6);
        private readonly static TimeSpan FiveSecondTimeSpan = new(0, 0, 5);
        private readonly static TimeSpan NowTimeSpan = new(0, 0, 0);
        #endregion Constants

        public static List<JobTimer> CreateSchedule(bool isOnUpdate, bool onlyRestarts, int interval, SchedulerManager scheduler)
        {
            if (interval > 24 || interval < 1)
            {
                throw new Exception("Interval needs to be between 1 and 24");
            }

            List<JobTimer> timers = new List<JobTimer>();
            DateTime now = DateTime.Now;
            TimeSpan restartTimeSpan = GetNextRestartTime(interval, now) - now;

            if (isOnUpdate)
            {
                bool timeToRestart = restartTimeSpan.TotalMinutes > 40;
                if (timeToRestart)
                {
                    TimeSpan restartTimeSpanUpdate = GetNextRestartTimeUpdate(now) - now;

                    if ((restartTimeSpanUpdate - TwentyMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_TWENTYMINUTES, 0, restartTimeSpanUpdate, TwentyMinuteTimeSpan));

                    if ((restartTimeSpanUpdate - FifteenMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_FIFTEENMINUTES, 0, restartTimeSpanUpdate, FifteenMinuteTimeSpan));

                    if ((restartTimeSpanUpdate - TenMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_TENMINUTES, 0, restartTimeSpanUpdate, TenMinuteTimeSpan));

                    if ((restartTimeSpanUpdate - FiveMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_FIVEMINUTES, 0, restartTimeSpanUpdate, FiveMinuteTimeSpan));

                    if ((restartTimeSpanUpdate - OneMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_ONEMINUTE, 0, restartTimeSpanUpdate, OneMinuteTimeSpan));

                    if ((restartTimeSpanUpdate - TenSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_LOCK, 0, restartTimeSpanUpdate, TenSecondTimeSpan));

                    if ((restartTimeSpanUpdate - NineSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_RESTARTINGNOW, 0, restartTimeSpanUpdate, NineSecondTimeSpan));

                    if ((restartTimeSpanUpdate - EightSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_RESTARTINGNOW, 0, restartTimeSpanUpdate, EightSecondTimeSpan));

                    if ((restartTimeSpanUpdate - SevenSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_RESTARTINGNOW, 0, restartTimeSpanUpdate, SevenSecondTimeSpan));

                    if ((restartTimeSpanUpdate - SixSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_RESTARTINGNOW, 0, restartTimeSpanUpdate, SixSecondTimeSpan));

                    if ((restartTimeSpanUpdate - FiveSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_UPDATE_RESTARTINGNOW, 0, restartTimeSpanUpdate, FiveSecondTimeSpan));

                    timers.Add(new JobTimer(scheduler, CMD_SHUTDOWN, 0, restartTimeSpanUpdate, NowTimeSpan));
                }
            }
            else
            {
                if (!onlyRestarts)
                {
                    if (interval > 1 && (restartTimeSpan - OneHourTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_ONEHOUR, interval, restartTimeSpan, OneHourTimeSpan));

                    if ((restartTimeSpan - ThirtyMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_THIRTYMINUTES, interval, restartTimeSpan, ThirtyMinuteTimeSpan));

                    if ((restartTimeSpan - FifteenMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_FIFTEENMINUTES, interval, restartTimeSpan, FifteenMinuteTimeSpan));

                    if ((restartTimeSpan - FiveMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_FIVEMINUTES, interval, restartTimeSpan, FiveMinuteTimeSpan));

                    if ((restartTimeSpan - OneMinuteTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_ONEMINUTE, interval, restartTimeSpan, OneMinuteTimeSpan));

                    if ((restartTimeSpan - NineSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_RESTARTINGNOW, interval, restartTimeSpan, NineSecondTimeSpan));

                    if ((restartTimeSpan - EightSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_RESTARTINGNOW, interval, restartTimeSpan, EightSecondTimeSpan));

                    if ((restartTimeSpan - SevenSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_RESTARTINGNOW, interval, restartTimeSpan, SevenSecondTimeSpan));

                    if ((restartTimeSpan - SixSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_RESTARTINGNOW, interval, restartTimeSpan, SixSecondTimeSpan));

                    if ((restartTimeSpan - FiveSecondTimeSpan).TotalSeconds > 0)
                        timers.Add(new JobTimer(scheduler, CMD_RESTARTINGNOW, interval, restartTimeSpan, FiveSecondTimeSpan));
                }

                if ((restartTimeSpan - TenSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(scheduler, CMD_LOCK, interval, restartTimeSpan, TenSecondTimeSpan));

                timers.Add(new JobTimer(scheduler, CMD_SHUTDOWN, interval, restartTimeSpan, NowTimeSpan));
            }

            return timers;
        }

        public static List<JobTimer> CreateCustomJobTimers(bool onlyRestarts, int interval, SchedulerManager scheduler, List<CustomMessage> customMessages)
        {
            List<JobTimer> timers = new List<JobTimer>();

            if (!onlyRestarts)
            {
                string restartMessage = string.Empty;
                if (interval == 1)
                {
                    restartMessage = "say -1 The server restarts every hour";
                }
                else
                {
                    restartMessage = $"say -1 The server restarts every {interval} hours";
                }
                timers.Add(new JobTimer(scheduler, restartMessage, false, FiveMinuteTimeSpan, FifteenMinuteTimeSpan));
            }

            foreach (CustomMessage customMessage in customMessages)
            {
                TimeSpan waitTime;
                if (customMessage.IsTimeOfDay)
                {
                    DateTime now = DateTime.Now;
                    waitTime = new DateTime(now.Year, now.Month, now.Day, customMessage.WaitTime["hours"], customMessage.WaitTime["minutes"], customMessage.WaitTime["seconds"]) - DateTime.Now;
                }
                else
                {
                    waitTime = new TimeSpan(customMessage.WaitTime["hours"], customMessage.WaitTime["minutes"], customMessage.WaitTime["seconds"]);
                }

                timers.Add(new JobTimer(
                    scheduler,
                    $"say -1 {customMessage.Message}",
                    customMessage.IsTimeOfDay,
                    waitTime,
                    new TimeSpan(customMessage.Interval["hours"], customMessage.Interval["minutes"], customMessage.Interval["seconds"]))
                );
            }

            return timers;
        }

        public static DateTime GetNextRestartTime(int interval, DateTime now)
        {
            for (int time = interval; time < 24; time += interval)
            {
                if (now.Hour < time)
                {
                    return new DateTime(now.Year, now.Month, now.Day, time, 0, 0);
                }
            }
            return new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);
        }

        public static DateTime GetNextRestartTimeUpdate(DateTime now)
        {
            if (now.Minute >= 0 && now.Minute < 5)
            {
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, 15, 0);
            }
            else if (now.Minute >= 5 && now.Minute < 20)
            {
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, 30, 0, 0);
            }
            else if (now.Minute >= 20 && now.Minute < 35)
            {
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, 45, 0, 0);
            }
            else if (now.Minute >= 35 && now.Minute < 50)
            {
                return new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0, 0);
            }
            else if (now.Minute >= 50 && now.Minute < 60)
            {
                return new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 15, 0, 0);
            }

            return new DateTime();
        }

        public static void UpdateExpansionScheduler(ManagerConfig config, NotificationSchedulerFile expansionScheduler)
        {
            int interval = config.restartInterval;
            expansionScheduler.Notifications = new List<NotificationItem>();

            string title = "Server Restart";
            string restartTitle = "Restart Information";
            string oneHourText = "The Server is restarting in 1 hour";
            string thirtyMinuteText = "The Server is restarting in 30 minutes";
            string fifteenMinuteText = "The Server is restarting in 15 minutes";
            string fiveMinuteText = "The Server is restarting in 5 minutes! Please land your helicopters as soon as possible!";
            string oneMinuteText = "The Server is restarting in 1 minute! Please log out in order to prevent inventory loss!";
            string restartingNowText = "The Server is restarting now!!";
            string icon = "Exclamationmark";
            string color = "";

            string restartNotice = "";

            List<CustomMessage> repeatMessages = new List<CustomMessage>();
            List<CustomMessage> afterRestartMessages = new List<CustomMessage>();
            List<CustomMessage> afterRestartRepeatMessages = new List<CustomMessage>();
            DateTime now = DateTime.Now;

            if (config.customMessages != null)
            {
                foreach (CustomMessage item in config.customMessages)
                {
                    if (item.Interval["hours"] == 0 && item.Interval["minutes"] == 0 && item.Interval["seconds"] == 0)
                    {
                        if (item.IsTimeOfDay)
                        {
                            expansionScheduler.Notifications.Add(new NotificationItem(Convert.ToInt32(item.WaitTime["hours"]), Convert.ToInt32(item.WaitTime["minutes"]), Convert.ToInt32(item.WaitTime["seconds"]), item.Title, item.Message, item.Icon, item.Color));
                        }
                        else
                        {
                            afterRestartMessages.Add(item);
                        }
                    }
                    else
                    {
                        int nextRestart = interval;
                        Dictionary<string, int> nextMessageTime = item.WaitTime;
                        if (item.IsTimeOfDay)
                        {
                            for (nextRestart = 0; nextMessageTime["hours"] > nextRestart; nextRestart += interval) ;
                        }
                        while (nextMessageTime["hours"] < 24)
                        {
                            while (nextMessageTime["hours"] < nextRestart)
                            {
                                expansionScheduler.Notifications.Add(new NotificationItem(Convert.ToInt32(nextMessageTime["hours"]), Convert.ToInt32(nextMessageTime["minutes"]), Convert.ToInt32(nextMessageTime["seconds"]), item.Title, item.Message, item.Icon, item.Color));
                                nextMessageTime["seconds"] += item.Interval["seconds"];
                                if (nextMessageTime["seconds"] >= 60)
                                {
                                    nextMessageTime["seconds"] -= 60;
                                    nextMessageTime["minutes"] += 1;
                                }
                                nextMessageTime["minutes"] += item.Interval["minutes"];
                                if (nextMessageTime["minutes"] >= 60)
                                {
                                    nextMessageTime["minutes"] -= 60;
                                    nextMessageTime["hours"] += 1;
                                }
                                nextMessageTime["hours"] += item.Interval["hours"];
                            }
                            nextMessageTime = item.WaitTime;
                            nextMessageTime["hours"] = nextRestart;
                            nextRestart += interval;
                        }
                    }
                }
            }

            if (interval == 1)
            {
                restartNotice = "The Server restarts every hour";
            }
            else
            {
                restartNotice = $"The Server restarts every {interval} hours";
            }

            for (int i = 0; i < 24; i++)
            {
                expansionScheduler.Notifications.Add(new NotificationItem(i, 5, 0, restartTitle, restartNotice, icon, color));
                expansionScheduler.Notifications.Add(new NotificationItem(i, 20, 0, restartTitle, restartNotice, icon, color));
                expansionScheduler.Notifications.Add(new NotificationItem(i, 35, 0, restartTitle, restartNotice, icon, color));
                expansionScheduler.Notifications.Add(new NotificationItem(i, 50, 0, restartTitle, restartNotice, icon, color));

                if (i % interval == interval - 1 || interval == 1)
                {
                    if (interval != 1)
                    {
                        expansionScheduler.Notifications.Add(new NotificationItem(i, 0, 0, title, oneHourText, icon, color));
                    }
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 30, 0, title, thirtyMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 45, 0, title, fifteenMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 55, 0, title, fiveMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 59, 0, title, oneMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 59, 50, title, restartingNowText, icon, color));
                }
                else if (i % interval == 0)
                {
                    foreach (CustomMessage item in afterRestartMessages)
                    {
                        expansionScheduler.Notifications.Add(new NotificationItem(i + Convert.ToInt32(item.WaitTime["hours"]), Convert.ToInt32(item.WaitTime["minutes"]), Convert.ToInt32(item.WaitTime["seconds"]), item.Title, item.Message, item.Icon, item.Color));
                    }
                }
            }
        }

        public static bool IsTimeToRestart(int interval)
        {
            DateTime currentTime = DateTime.Now;
            if (currentTime.Hour % interval == interval - 1)
            {
                if (currentTime.Minute >= 0 && currentTime.Minute < 5
                    || currentTime.Minute >= 5 && currentTime.Minute < 15)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (currentTime.Minute >= 50 && currentTime.Minute < 60
                    || currentTime.Minute >= 0 && currentTime.Minute < 5
                    || currentTime.Minute >= 5 && currentTime.Minute < 20
                    || currentTime.Minute >= 20 && currentTime.Minute < 35
                    || currentTime.Minute >= 35 && currentTime.Minute < 50)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}