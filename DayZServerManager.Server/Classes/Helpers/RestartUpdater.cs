using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;

namespace DayZServerManager.Server.Classes.Helpers
{
    public static class RestartUpdater
    {
        public static SchedulerFile CreateNewSchedulerFile(bool isOnUpdate, bool onlyRestarts, int interval, List<JobItem> jobItems)
        {
            if (isOnUpdate)
            {
                return CreateOnUpdateSchedulerFile(interval);
            }
            else
            {
                if (onlyRestarts)
                {
                    return CreateOnlyRestartsSchedulerFile(interval, jobItems);
                }
                else
                {
                    return CreateFullSchedulerFile(interval, jobItems);
                }
            }
        }
         
        private static SchedulerFile CreateFullSchedulerFile(int interval, List<JobItem> jobItems)
        {
            SchedulerFile sch = new SchedulerFile();
            sch.JobItems = new List<JobItem>();
            int id = 0;

            foreach (JobItem item in jobItems)
            {
                item.ID = id;
                sch.JobItems.Add(item);
                id++;
            }

            if (interval > 0)
            {
                Dictionary<string, double> runtime = new Dictionary<string, double>
                {
                    {"hours", 0},
                    {"minutes", 0},
                    {"seconds", 0}

                };
                Dictionary<string, double> runtimeDCmsg = new Dictionary<string, double>
                {
                    {"hours", 0},
                    {"minutes", 20},
                    {"seconds", 0}

                };
                int loop = 0;
                int restartNowLoop = 5;
                string cmdShutdown = "#shutdown";
                string cmdLock = "#lock";
                string oneHourCmd = "say -1 Alert: The Server is restarting in 1 hour";
                string thirtyMinuteCmd = "say -1 Alert: The Server is restarting in 30 minutes";
                string fifteenMinuteCmd = "say -1 Alert: The Server is restarting in 15 minutes";
                string fiveMinuteCmd = "say -1 Alert: The Server is restarting in 5 minutes! Please land your helicopters as soon as possible!";
                string oneMinuteCmd = "say -1 Alert: The Server is restarting in 1 minute! Please log out in order to prevent inventory loss!";
                string restartingNowCmd = "say -1 Alert: The Server is restarting now!!";

                string serverRestartNotice = "";
                if (interval == 1)
                {
                    serverRestartNotice = $"say -1 The Server restarts every hour";
                }
                else
                {
                    serverRestartNotice = $"say -1 The Server restarts every {interval} hours";
                }

                sch.JobItems.Add(new JobItem(id, false, new Dictionary<string, double> { { "hours", 0 }, { "minutes", 10 }, { "seconds", 0 } }, runtimeDCmsg, -1, serverRestartNotice));
                id++;

                for (int i = 0; i < 24; i++)
                {
                    if (i % interval == interval - 1 || interval == 1)
                    {
                        if (interval == 1)
                        {
                            sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 0 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                            id++;
                        }
                        else
                        {
                            sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 0 }, { "seconds", 0 } }, runtime, loop, oneHourCmd));
                            id++;
                        }
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 30 }, { "seconds", 0 } }, runtime, loop, thirtyMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 45 }, { "seconds", 0 } }, runtime, loop, fifteenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 55 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 59 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 59 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 59 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));

                    }
                    else if (i % interval == 0)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 0 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        id++;
                    }
                }
            }
            return sch;
        }

        private static SchedulerFile CreateOnlyRestartsSchedulerFile(int interval, List<JobItem> jobItems)
        {
            SchedulerFile sch = new SchedulerFile();
            sch.JobItems = new List<JobItem>();
            int id = 0;

            foreach (JobItem item in jobItems)
            {
                item.ID = id;
                sch.JobItems.Add(item);
                id++;
            }

            if (interval > 0)
            {
                Dictionary<string, double> runtime = new Dictionary<string, double>
                {
                    { "hours", 0 },
                    { "minutes", 0 },
                    { "seconds", 0 }
                };
                int loop = 0;
                string cmdShutdown = "#shutdown";
                string cmdLock = "#lock";

                for (int i = 0; i < 24; i++)
                {
                    if (i % interval == interval - 1 || interval == 1)
                    {
                        if (interval == 1)
                        {
                            sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 0 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                            id++;
                        }
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 59 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                    }
                    else if (i % interval == 0)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", i }, { "minutes", 0 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        id++;
                    }
                }
            }
            return sch;
        }

        private static SchedulerFile CreateOnUpdateSchedulerFile(int interval)
        {
            SchedulerFile sch = new SchedulerFile();
            sch.JobItems = new List<JobItem>();
            if (interval > 0)
            {
                int id = 0;
                Dictionary<string, double> runtime = new Dictionary<string, double>
                {
                    { "hours", 0 },
                    { "minutes", 0 },
                    { "seconds", 0 }
                };
                int loop = 0;
                int restartNowLoop = 5;
                string cmdShutdown = "#shutdown";
                string cmdLock = "#lock";
                string twentyMinuteCmd = "say -1 Alert: The Server is restarting in 20 minutes to load updated mods! Please restart your game afterwards!";
                string fifteenMinuteCmd = "say -1 Alert: The Server is restarting in 15 minutes to load updated mods! Please restart your game afterwards!";
                string tenMinuteCmd = "say -1 Alert: The Server is restarting in 10 minutes to load updated mods! Please restart your game afterwards!";
                string fiveMinuteCmd = "say -1 Alert: The Server is restarting in 5 minutes to load updated mods! ! Please land your helicopters as soon as possible and restart your game afterwards!";
                string oneMinuteCmd = "say -1 Alert: The Server is restarting in 1 minute to load updated mods! ! Please log out in order to prevent inventory loss and restart your game afterwards!";
                string restartingNowCmd = "say -1 Alert: The Server is restarting now to load updated mods!! Please your restart your game afterwards!";
                DateTime currentTime = DateTime.Now;

                double hour = currentTime.Hour;

                double nextHour = currentTime.AddHours(1).Hour;

                if (hour % interval == interval - 1 || interval == 1)
                {
                    if (currentTime.Minute >= 0 && currentTime.Minute < 5)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 5 }, { "seconds", 0 } }, runtime, loop, tenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 10 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 14 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 14 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 14 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 15 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        return sch;
                    }
                    else if (currentTime.Minute >= 5 && currentTime.Minute < 20)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 10 }, { "seconds", 0 } }, runtime, loop, twentyMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 15 }, { "seconds", 0 } }, runtime, loop, fifteenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 20 }, { "seconds", 0 } }, runtime, loop, tenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 25 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 29 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 29 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 29 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 30 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        return sch;
                    }
                }
                else
                {
                    if (currentTime.Minute >= 0 && currentTime.Minute < 5)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 5 }, { "seconds", 0 } }, runtime, loop, tenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 10 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 14 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 14 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 14 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 15 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        return sch;
                    }
                    else if (currentTime.Minute >= 5 && currentTime.Minute < 20)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 10 }, { "seconds", 0 } }, runtime, loop, twentyMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 15 }, { "seconds", 0 } }, runtime, loop, fifteenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 20 }, { "seconds", 0 } }, runtime, loop, tenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 25 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 29 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 29 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 29 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 30 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        return sch;
                    }
                    else if (currentTime.Minute >= 20 && currentTime.Minute < 35)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 25 }, { "seconds", 0 } }, runtime, loop, twentyMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 30 }, { "seconds", 0 } }, runtime, loop, fifteenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 35 }, { "seconds", 0 } }, runtime, loop, tenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 40 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 44 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 44 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 44 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 45 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        return sch;
                    }
                    else if (currentTime.Minute >= 35 && currentTime.Minute < 50)
                    {
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 40 }, { "seconds", 0 } }, runtime, loop, twentyMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 45 }, { "seconds", 0 } }, runtime, loop, fifteenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 50 }, { "seconds", 0 } }, runtime, loop, tenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 55 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 59 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 59 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 59 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 0 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        return sch;
                    }
                    else if (currentTime.Minute >= 50 && currentTime.Minute < 60)
                    {

                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", hour }, { "minutes", 55 }, { "seconds", 0 } }, runtime, loop, twentyMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 0 }, { "seconds", 0 } }, runtime, loop, fifteenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 5 }, { "seconds", 0 } }, runtime, loop, tenMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 10 }, { "seconds", 0 } }, runtime, loop, fiveMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 14 }, { "seconds", 0 } }, runtime, loop, oneMinuteCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 14 }, { "seconds", 0 } }, runtime, loop, cmdLock));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 14 }, { "seconds", 50 } }, runtime, restartNowLoop, restartingNowCmd));
                        id++;
                        sch.JobItems.Add(new JobItem(id, true, new Dictionary<string, double> { { "hours", nextHour }, { "minutes", 15 }, { "seconds", 0 } }, runtime, loop, cmdShutdown));
                        return sch;
                    }
                }
            }
            return sch;
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
                        Dictionary<string, double> nextMessageTime = item.WaitTime;
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
    }
}