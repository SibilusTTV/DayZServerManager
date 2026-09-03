using Application.Handlers;
using Application.IService;
using Domain.Manager;
using Domain.Profile;
using Domain.Scheduler;

namespace Application.Service;

public class RestartUpdaterService : IRestartUpdaterService
{
    #region Constants
    private const string CmdShutdown = "#shutdown";
    private const string CmdLock = "#lock";
    private const string CmdOneHour = "say -1 Alert: The Server is restarting in 1 hour";
    private const string CmdThirtyMinutes = "say -1 Alert: The Server is restarting in 30 minutes";
    private const string CmdFifteenMinutes = "say -1 Alert: The Server is restarting in 15 minutes";
    private const string CmdFiveMinutes = "say -1 Alert: The Server is restarting in 5 minutes! Please land your helicopters as soon as possible!";
    private const string CmdOneMinute = "say -1 Alert: The Server is restarting in 1 minute! Please log out in order to prevent inventory loss!";
    private const string CmdRestartingNow = "say -1 Alert: The Server is restarting now!!";

    private const string CmdUpdateTwentyMinutes = "say -1 Alert: The Server is restarting in 20 minutes to load updated mods! Please restart your game afterwards!";
    private const string CmdUpdateFifteenMinutes = "say -1 Alert: The Server is restarting in 15 minutes to load updated mods! Please restart your game afterwards!";
    private const string CmdUpdateTenMinutes = "say -1 Alert: The Server is restarting in 10 minutes to load updated mods! Please restart your game afterwards!";
    private const string CmdUpdateFiveMinutes = "say -1 Alert: The Server is restarting in 5 minutes to load updated mods! ! Please land your helicopters as soon as possible and restart your game afterwards!";
    private const string CmdUpdateOneMinute = "say -1 Alert: The Server is restarting in 1 minute to load updated mods! ! Please log out in order to prevent inventory loss and restart your game afterwards!";
    private const string CmdUpdateRestartingNow = "say -1 Alert: The Server is restarting now to load updated mods!! Please your restart your game afterwards!";
    
    private readonly TimeSpan _oneHourTimeSpan = new TimeSpan(1, 0, 0);
    private readonly TimeSpan _thirtyMinuteTimeSpan = new TimeSpan(0, 30, 0);
    private readonly TimeSpan _twentyMinuteTimeSpan = new TimeSpan(0, 20, 0);
    private readonly TimeSpan _fifteenMinuteTimeSpan = new TimeSpan(0, 15, 0);
    private readonly TimeSpan _tenMinuteTimeSpan = new TimeSpan(0, 10, 0);
    private readonly TimeSpan _fiveMinuteTimeSpan = new TimeSpan(0, 5, 0);
    private readonly TimeSpan _oneMinuteTimeSpan = new TimeSpan(0, 1, 0);
    private readonly TimeSpan _tenSecondTimeSpan = new TimeSpan(0, 0, 10);
    private readonly TimeSpan _nineSecondTimeSpan = new TimeSpan(0, 0, 9);
    private readonly TimeSpan _eightSecondTimeSpan = new TimeSpan(0, 0, 8);
    private readonly TimeSpan _sevenSecondTimeSpan = new TimeSpan(0, 0, 7);
    private readonly TimeSpan _sixSecondTimeSpan = new TimeSpan(0, 0, 6);
    private readonly TimeSpan _fiveSecondTimeSpan = new TimeSpan(0, 0, 5);
    private readonly TimeSpan _nowTimeSpan = new TimeSpan(0, 0, 0);
    #endregion Constants
    
    public RestartUpdaterService()
    {
        
    }

    public List<JobTimer> CreateSchedule(bool isOnUpdate, bool onlyRestarts, int interval, Action<string> sendCommand, Func<bool> isConnected)
    {
        if (interval > 24 || interval < 1)
        {
            throw new Exception("Interval needs to be between 1 and 24");
        }

        List<JobTimer> timers = [];
        var now = DateTime.Now;
        var restartTimeSpan = GetNextRestartTime(interval, now) - now;

        if (isOnUpdate)
        {
            var timeToRestart = restartTimeSpan.TotalMinutes > 40;
            if (timeToRestart)
            {
                var restartTimeSpanUpdate = GetNextRestartTimeUpdate(now) - now;

                if ((restartTimeSpanUpdate - _twentyMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateTwentyMinutes, 0, restartTimeSpanUpdate, _twentyMinuteTimeSpan));

                if ((restartTimeSpanUpdate - _fifteenMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateFifteenMinutes, 0, restartTimeSpanUpdate, _fifteenMinuteTimeSpan));

                if ((restartTimeSpanUpdate - _tenMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateTenMinutes, 0, restartTimeSpanUpdate, _tenMinuteTimeSpan));

                if ((restartTimeSpanUpdate - _fiveMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateFiveMinutes, 0, restartTimeSpanUpdate, _fiveMinuteTimeSpan));

                if ((restartTimeSpanUpdate - _oneMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateOneMinute, 0, restartTimeSpanUpdate, _oneMinuteTimeSpan));

                if ((restartTimeSpanUpdate - _tenSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdLock, 0, restartTimeSpanUpdate, _tenSecondTimeSpan));

                if ((restartTimeSpanUpdate - _nineSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateRestartingNow, 0, restartTimeSpanUpdate, _nineSecondTimeSpan));

                if ((restartTimeSpanUpdate - _eightSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateRestartingNow, 0, restartTimeSpanUpdate, _eightSecondTimeSpan));

                if ((restartTimeSpanUpdate - _sevenSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateRestartingNow, 0, restartTimeSpanUpdate, _sevenSecondTimeSpan));

                if ((restartTimeSpanUpdate - _sixSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateRestartingNow, 0, restartTimeSpanUpdate, _sixSecondTimeSpan));

                if ((restartTimeSpanUpdate - _fiveSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdUpdateRestartingNow, 0, restartTimeSpanUpdate, _fiveSecondTimeSpan));

                timers.Add(new JobTimer(sendCommand, isConnected, CmdShutdown, 0, restartTimeSpanUpdate, _nowTimeSpan));
            }
        }
        else
        {
            if (!onlyRestarts)
            {
                if (interval > 1 && (restartTimeSpan - _oneHourTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdOneHour, interval, restartTimeSpan, _oneHourTimeSpan));

                if ((restartTimeSpan - _thirtyMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdThirtyMinutes, interval, restartTimeSpan, _thirtyMinuteTimeSpan));

                if ((restartTimeSpan - _fifteenMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdFifteenMinutes, interval, restartTimeSpan, _fifteenMinuteTimeSpan));

                if ((restartTimeSpan - _fiveMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdFiveMinutes, interval, restartTimeSpan, _fiveMinuteTimeSpan));

                if ((restartTimeSpan - _oneMinuteTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdOneMinute, interval, restartTimeSpan, _oneMinuteTimeSpan));

                if ((restartTimeSpan - _nineSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdRestartingNow, interval, restartTimeSpan, _nineSecondTimeSpan));

                if ((restartTimeSpan - _eightSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdRestartingNow, interval, restartTimeSpan, _eightSecondTimeSpan));

                if ((restartTimeSpan - _sevenSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdRestartingNow, interval, restartTimeSpan, _sevenSecondTimeSpan));

                if ((restartTimeSpan - _sixSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdRestartingNow, interval, restartTimeSpan, _sixSecondTimeSpan));

                if ((restartTimeSpan - _fiveSecondTimeSpan).TotalSeconds > 0)
                    timers.Add(new JobTimer(sendCommand, isConnected, CmdRestartingNow, interval, restartTimeSpan, _fiveSecondTimeSpan));
            }

            if ((restartTimeSpan - _tenSecondTimeSpan).TotalSeconds > 0)
                timers.Add(new JobTimer(sendCommand, isConnected, CmdLock, interval, restartTimeSpan, _tenSecondTimeSpan));

            timers.Add(new JobTimer(sendCommand, isConnected, CmdShutdown, interval, restartTimeSpan, _nowTimeSpan));
        }

        return timers;
    }

    public List<JobTimer> CreateCustomJobTimers(bool onlyRestarts, int interval, Action<string> sendCommand, Func<bool> isConnected, List<CustomMessage> customMessages)
    {
        List<JobTimer> timers = [];

        if (!onlyRestarts)
        {
            var restartMessage = string.Empty;
            if (interval == 1)
            {
                restartMessage = "say -1 The server restarts every hour";
            }
            else
            {
                restartMessage = $"say -1 The server restarts every {interval} hours";
            }
            timers.Add(new JobTimer(sendCommand, isConnected, restartMessage, false, _fiveMinuteTimeSpan, _fifteenMinuteTimeSpan));
        }

        foreach (CustomMessage customMessage in customMessages)
        {
            TimeSpan waitTime;
            if (customMessage.IsTimeOfDay)
            {
                DateTime now = DateTime.Now;
                waitTime = new DateTime(now.Year, now.Month, now.Day, customMessage.WaitTime.Hours, customMessage.WaitTime.Minutes, customMessage.WaitTime.Seconds) - DateTime.Now;
            }
            else
            {
                waitTime = new TimeSpan(customMessage.WaitTime.Hours, customMessage.WaitTime.Minutes, customMessage.WaitTime.Seconds);
            }

            timers.Add(new JobTimer(
                sendCommand, 
                isConnected,
                $"say -1 {customMessage.Message}",
                customMessage.IsTimeOfDay,
                waitTime,
                new TimeSpan(customMessage.Interval.Hours, customMessage.Interval.Minutes, customMessage.Interval.Seconds))
            );
        }

        return timers;
    }

    public DateTime GetNextRestartTime(int interval, DateTime now)
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

    public DateTime GetNextRestartTimeUpdate(DateTime now)
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

    public void UpdateExpansionScheduler(SchedulerConfig config, NotificationSchedulerFile expansionScheduler)
    {
        var interval = config.restartInterval;
        expansionScheduler.Notifications = [];

        const string title = "Server Restart";
        const string restartTitle = "Restart Information";
        const string oneHourText = "The Server is restarting in 1 hour";
        const string thirtyMinuteText = "The Server is restarting in 30 minutes";
        const string fifteenMinuteText = "The Server is restarting in 15 minutes";
        const string fiveMinuteText = "The Server is restarting in 5 minutes! Please land your helicopters as soon as possible!";
        const string oneMinuteText = "The Server is restarting in 1 minute! Please log out in order to prevent inventory loss!";
        const string restartingNowText = "The Server is restarting now!!";
        const string icon = "Exclamationmark";
        const string color = "";

        var restartNotice = "";

        List<CustomMessage> afterRestartMessages = [];
        var now = DateTime.Now;

        foreach (var message in config.customMessages)
        {
            if (message.Interval.Hours == 0 && message.Interval.Minutes == 0 && message.Interval.Seconds == 0)
            {
                if (message.IsTimeOfDay)
                {        
                    expansionScheduler.Notifications.Add(new NotificationItem(message.WaitTime.Hours, message.WaitTime.Minutes, message.WaitTime.Seconds, message.Title, message.Message, message.Icon, message.Color));
                }
                else
                {
                    expansionScheduler.Notifications.Add(new NotificationItem(now.Hour + message.WaitTime.Hours, now.Minute + message.WaitTime.Minutes, now.Second + message.WaitTime.Seconds, message.Title, message.Message, message.Icon, message.Color));
                }
            }
            else
            {
                if (message.IsTimeOfDay)
                {
                    var time = new TimeSpan(message.WaitTime.Hours, message.WaitTime.Minutes, message.WaitTime.Seconds);
                    while (time.Hours > 24)
                    {
                        expansionScheduler.Notifications.Add(new NotificationItem(time.Hours, time.Minutes, time.Seconds, message.Title, message.Message, message.Icon, message.Color));
                        time = time.Add(message.Interval);
                    }
                }
                else
                {
                    var time = new TimeSpan(now.Hour, now.Minute, now.Second).Add(message.WaitTime);
                    while (time.Hours > 24)
                    {
                        expansionScheduler.Notifications.Add(new NotificationItem(time.Hours, time.Minutes, time.Seconds, message.Title, message.Message, message.Icon, message.Color));
                        time = time.Add(message.Interval);
                    }
                }
            }
        }

        restartNotice = interval == 1 ? "The Server restarts every hour" : $"The Server restarts every {interval} hours";

        for (var i = 0; i < 24; i++)
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
                foreach (var item in afterRestartMessages)
                {
                    expansionScheduler.Notifications.Add(new NotificationItem(i + Convert.ToInt32(item.WaitTime.Hours), Convert.ToInt32(item.WaitTime.Minutes), Convert.ToInt32(item.WaitTime.Seconds), item.Title, item.Message, item.Icon, item.Color));
                }
            }
        }
    }

    public bool IsTimeToRestart(int interval)
    {
        var currentTime = DateTime.Now;
        if (currentTime.Hour % interval == interval - 1)
        {
            return currentTime.Minute is >= 0 and < 5 or >= 5 and < 15;
        }
        else
        {
            return currentTime.Minute is >= 50 and < 60 or >= 0 and < 5 or >= 5 and < 20 or >= 20 and < 35 or >= 35 and < 50;
        }
    }
}