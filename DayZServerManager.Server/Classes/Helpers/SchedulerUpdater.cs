using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;

namespace DayZServerManager.Server.Classes.Helpers
{
    internal static class RestartUpdater
    {
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
                            for (nextRestart = 0; nextMessageTime["hours"] < nextRestart; nextRestart += interval);
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
