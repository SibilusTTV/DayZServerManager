using DayZServerManager.BecClasses;
using DayZServerManager.ProfileClasses.NotificationSchedulerClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Helpers
{
    internal class RestartUpdater
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

        private static void UpdateWithoutExpansion(int interval, SchedulerFile becScheduler)
        {
            becScheduler.JobItems = new List<JobItem>();

            int id = 0;
            string days = "1,2,3,4,5,6,7";
            string runtime = "000000";
            int loop = 0;
            string cmd = "#shutdown";
            string oneHourCmd = "say -1 Alert: The Server is restarting in 1 hour";
            string thirtyMinuteCmd = "say -1 Alert: The Server is restarting in 30 minutes";
            string fifteenMinuteCmd = "say -1 Alert: The Server is restarting in 15 minutes";
            string fiveMinuteCmd = "say -1 Alert: The Server is restarting in 5 minutes";
            string oneMinuteCmd = "say -1 Alert: The Server is restarting in 1 minute";
            string restartingNowCmd = "say -1 Alert: The Server is restarting now!!";

            for (int i = 0; i < 24; i++)
            {
                string hour = "";
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
                    becScheduler.JobItems.Add(new JobItem(id, days, $"{hour}:59:50", runtime, loop, restartingNowCmd));
                    id++;

                }
                else if (i % interval == 0)
                {
                    string start = $"{hour}:00:00";
                    becScheduler.JobItems.Add(new JobItem(id, days, start, runtime, loop, cmd));
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
            string fiveMinuteText = "The Server is restarting in 5 minutes";
            string oneMinuteText = "The Server is restarting in 1 minute";
            string restartingNowText = "The Server is restarting now!!";
            string icon = "Exclamationmark";
            string color = "";

            becScheduler.JobItems = new List<JobItem>();

            int id = 0;
            string days = "1,2,3,4,5,6,7";
            string runtime = "000000";
            int loop = 0;
            string cmd = "#shutdown";

            for (int i = 0; i < 24; i++)
            {
                if (i % interval == interval - 1)
                {
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 0, 0, title, oneHourText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 30, 0, title, thirtyMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 45, 0, title, fifteenMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 55, 0, title, fiveMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 59, 0, title, oneMinuteText, icon, color));
                    expansionScheduler.Notifications.Add(new NotificationItem(i, 59, 50, title, restartingNowText, icon, color));
                }
                else if (i % interval == 0)
                {
                    string start = "";
                    if (i < 10)
                    {
                        start = $"0{i}:00:00";
                    }
                    else
                    {
                        start = $"{i}:00:00";
                    }
                    becScheduler.JobItems.Add(new JobItem(id, days, start, runtime, loop, cmd));
                    id++;
                }
            }
        }
    }
}
