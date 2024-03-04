using DayZServerManager.BecClasses;
using DayZServerManager.ProfileClasses.NotificationSchedulerClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Helpers
{
    internal static class RestartUpdater
    {
        public static void UpdateRestartScripts(int interval, SchedulerFile? becScheduler, NotificationSchedulerFile? expansionScheduler = null)
        {
            if (expansionScheduler == null && becScheduler != null)
            {
                UpdateWithoutExpansion(interval, becScheduler);
            }
            else if (expansionScheduler != null && becScheduler != null)
            {
                UpdateWithExpansion(interval, becScheduler, expansionScheduler);
            }
        }

        public static bool UpdateOnUpdateRestartScript(int interval, DateTime currentTime, SchedulerFile? becUpdateScheduler)
        {
            if (becUpdateScheduler != null)
            {
                becUpdateScheduler.JobItems = new List<JobItem>();

                int id = 0;
                string days = "1,2,3,4,5,6,7";
                string runtime = "000000";
                int loop = 0;
                string cmd = "#shutdown";
                string twentyMinuteCmd = "say -1 Alert: The Server is restarting in 20 minutes to load updated mods! Please restart your game afterwards!";
                string fifteenMinuteCmd = "say -1 Alert: The Server is restarting in 15 minutes to load updated mods! Please restart your game afterwards!";
                string tenMinuteCmd = "say -1 Alert: The Server is restarting in 10 minutes to load updated mods! Please restart your game afterwards!";
                string fiveMinuteCmd = "say -1 Alert: The Server is restarting in 5 minutes to load updated mods! ! Please land your helicopters as soon as possible and restart your game afterwards!";
                string oneMinuteCmd = "say -1 Alert: The Server is restarting in 1 minute to load updated mods! ! Please log out in order to prevent inventory loss and restart your game afterwards!";
                string restartingNowCmd = "say -1 Alert: The Server is restarting now to load updated mods!! Please your restart your game afterwards!";

                string hour;
                if (currentTime.Hour < 10)
                {
                    hour = $"0{currentTime.Hour}";
                }
                else
                {
                    hour = $"{currentTime.Hour}";
                }

                string nextHour;
                if (currentTime.Hour == 23)
                {
                    nextHour = "00";
                }
                else if (currentTime.Hour < 9)
                {
                    nextHour = $"0{currentTime.Hour + 1}";
                }
                else
                {
                    nextHour = $"{currentTime.Hour + 1}";
                }

                string previousHour;
                if (currentTime.Hour == 0)
                {
                    previousHour = "23";
                }
                else if (currentTime.Hour < 11)
                {
                    previousHour = $"0{currentTime.Hour}";
                }
                else
                {
                    previousHour = $"{currentTime.Hour}";
                }

                if (currentTime.Hour % interval == interval - 1)
                {
                    if (currentTime.Minute >= 0 && currentTime.Minute < 5)
                    {
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{previousHour}:55:00", runtime, loop, twentyMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:00:00", runtime, loop, fifteenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:05:00", runtime, loop, tenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:10:00", runtime, loop, fiveMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:14:00", runtime, loop, oneMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:14:50", runtime, loop, restartingNowCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:15:00", runtime, loop, cmd));
                        return true;
                    }
                    else if (currentTime.Minute >= 5 && currentTime.Minute < 20)
                    {
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:10:00", runtime, loop, twentyMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:15:00", runtime, loop, fifteenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:20:00", runtime, loop, tenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:25:00", runtime, loop, fiveMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:29:00", runtime, loop, oneMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:29:50", runtime, loop, restartingNowCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:30:00", runtime, loop, cmd));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (currentTime.Minute >= 50 && currentTime.Minute < 60)
                    {

                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:55:00", runtime, loop, twentyMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{nextHour}:00:00", runtime, loop, fifteenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{nextHour}:05:00", runtime, loop, tenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{nextHour}:10:00", runtime, loop, fiveMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{nextHour}:14:00", runtime, loop, oneMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{nextHour}:14:50", runtime, loop, restartingNowCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{nextHour}:15:00", runtime, loop, cmd));
                        return true;
                    }
                    else if (currentTime.Minute >= 0 && currentTime.Minute < 5)
                    {
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{previousHour}:55:00", runtime, loop, twentyMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:00:00", runtime, loop, fifteenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:05:00", runtime, loop, tenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:10:00", runtime, loop, fiveMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:14:00", runtime, loop, oneMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:14:50", runtime, loop, restartingNowCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:15:00", runtime, loop, cmd));
                        return true;
                    }
                    else if (currentTime.Minute >= 5 && currentTime.Minute < 20)
                    {
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:10:00", runtime, loop, twentyMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:15:00", runtime, loop, fifteenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:20:00", runtime, loop, tenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:25:00", runtime, loop, fiveMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:29:00", runtime, loop, oneMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:29:50", runtime, loop, restartingNowCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:30:00", runtime, loop, cmd));
                        return true;
                    }
                    else if (currentTime.Minute >= 20 && currentTime.Minute < 35)
                    {
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:25:00", runtime, loop, twentyMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:30:00", runtime, loop, fifteenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:35:00", runtime, loop, tenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:40:00", runtime, loop, fiveMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:44:00", runtime, loop, oneMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:44:50", runtime, loop, restartingNowCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:45:00", runtime, loop, cmd));
                        return true;
                    }
                    else if (currentTime.Minute >= 35 && currentTime.Minute < 50)
                    {
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:40:00", runtime, loop, twentyMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:45:00", runtime, loop, fifteenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:50:00", runtime, loop, tenMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:55:00", runtime, loop, fiveMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:59:00", runtime, loop, oneMinuteCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:59:50", runtime, loop, restartingNowCmd));
                        id++;
                        becUpdateScheduler.JobItems.Add(new JobItem(id, days, $"{nextHour}:00:00", runtime, loop, cmd));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private static void UpdateWithoutExpansion(int interval, SchedulerFile becScheduler)
        {
            becScheduler.JobItems = new List<JobItem>();

            int id = 0;
            string days = "1,2,3,4,5,6,7";
            string runtime = "000000";
            int loop = 0;
            string cmdShutdown = "#shutdown";
            string cmdLock = "#lock";
            string oneHourCmd = "say -1 Alert: The Server is restarting in 1 hour";
            string thirtyMinuteCmd = "say -1 Alert: The Server is restarting in 30 minutes";
            string fifteenMinuteCmd = "say -1 Alert: The Server is restarting in 15 minutes";
            string fiveMinuteCmd = "say -1 Alert: The Server is restarting in 5 minutes! Please land your helicopters as soon as possible!";
            string oneMinuteCmd = "say -1 Alert: The Server is restarting in 1 minute! Please log out in order to prevent inventory loss!";
            string restartingNowCmd = "say -1 Alert: The Server is restarting now!!";

            string joinDCmsg = "say -1 Press P for more information on the server or ask on Discord!";

            becScheduler.JobItems.Add(new JobItem(id, days, "001000", "002000", -1, joinDCmsg));
            id++;

            for (int i = 0; i < 24; i++)
            {
                string hour;
                if (i < 10)
                {
                    hour = $"0{i}";
                }
                else
                {
                    hour = $"{i}";
                }

                if (i % interval == interval - 1)
                {
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:00:00", runtime, loop, oneHourCmd));
                    id++;
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:30:00", runtime, loop, thirtyMinuteCmd));
                    id++;
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:45:00", runtime, loop, fifteenMinuteCmd));
                    id++;
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:55:00", runtime, loop, fiveMinuteCmd));
                    id++;
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:59:00", runtime, loop, oneMinuteCmd));
                    id++;
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:59:00", runtime, loop, cmdLock));
                    id++;
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:59:50", runtime, loop, restartingNowCmd));
                    id++;

                }
                else if (i % interval == 0)
                {
                    string start = $"{hour}:00:00";
                    becScheduler.JobItems.Add(new JobItem(id, days, start, runtime, loop, cmdShutdown));
                    id++;
                }
            }
        }

        private static void UpdateWithExpansion(int interval, SchedulerFile becScheduler, NotificationSchedulerFile expansionScheduler)
        {
            expansionScheduler.Notifications = new List<NotificationItem>();

            string title = "Server Restart";
            string oneHourText = "The Server is restarting in 1 hour";
            string thirtyMinuteText = "The Server is restarting in 30 minutes";
            string fifteenMinuteText = "The Server is restarting in 15 minutes";
            string fiveMinuteText = "The Server is restarting in 5 minutes! Please land your helicopters as soon as possible!";
            string oneMinuteText = "The Server is restarting in 1 minute! Please log out in order to prevent inventory loss!";
            string restartingNowText = "The Server is restarting now!!";
            string icon = "Exclamationmark";
            string color = "";

            string joinDCmsg = "say -1 Press P for more information on the server or ask on Discord!";

            becScheduler.JobItems = new List<JobItem>();

            int id = 0;
            string days = "1,2,3,4,5,6,7";
            string runtime = "000000";
            int loop = 0;
            string cmdShutdown = "#shutdown";
            string cmdLock = "#lock";



            for (int i = 0; i < 24; i++)
            {
                expansionScheduler.Notifications.Add(new NotificationItem(i, 10, 0, title, joinDCmsg, icon, color));
                expansionScheduler.Notifications.Add(new NotificationItem(i, 30, 0, title, joinDCmsg, icon, color));
                expansionScheduler.Notifications.Add(new NotificationItem(i, 50, 0, title, joinDCmsg, icon, color));

                if (i % interval == interval - 1)
                {
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 0, 0, title, oneHourText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 30, 0, title, thirtyMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 45, 0, title, fifteenMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 55, 0, title, fiveMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 59, 0, title, oneMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 59, 50, title, restartingNowText, icon, color));
                    string start;
                    if (i < 10)
                    {
                        start = $"0{i}:59:00";
                    }
                    else
                    {
                        start = $"{i}:59:00";
                    }
                    becScheduler.JobItems.Add(new JobItem(id, days, start, runtime, loop, cmdLock));
                    id++;
                }
                else if (i % interval == 0)
                {
                    string start;
                    if (i < 10)
                    {
                        start = $"0{i}:00:00";
                    }
                    else
                    {
                        start = $"{i}:00:00";
                    }
                    becScheduler.JobItems.Add(new JobItem(id, days, start, runtime, loop, cmdShutdown));
                    id++;
                }
            }
        }
    }
}
