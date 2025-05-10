
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;

namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class JobTimer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Timer _timer;
        private SchedulerManager _scheduler;

        public JobTimer(SchedulerManager scheduler, string cmd, int interval, TimeSpan restartTimeSpan, TimeSpan timeBeforeRestart)
        {
            _scheduler = scheduler;

            DateTime now = DateTime.Now;

            _timer = new Timer((state) => { ExecuteFunction(cmd, interval, timeBeforeRestart); },
                null,
                restartTimeSpan - timeBeforeRestart,
                new TimeSpan(interval, 0, 0)
            );
        }

        public JobTimer(SchedulerManager scheduler, string cmd, bool isTimeOfDay, TimeSpan waitTime, TimeSpan interval)
        {
            this._scheduler = scheduler;

            DateTime now = DateTime.Now;

            if (isTimeOfDay && interval.TotalSeconds == 0)
            {
                interval = new TimeSpan(24, 0, 0);
            }

            _timer = new Timer((state) => { ExecuteFunction(cmd, interval); }, null, waitTime, interval);
        }

        private void ExecuteFunction(string cmd, int interval, TimeSpan timeToRestart)
        {
            if (_scheduler.IsConnected())
            {
                Logger.Info($"Sending command {cmd}");
                _scheduler.SendCommand(cmd);
            }

            if (interval == 0)
            {
                _timer.Dispose();
            }
            else
            {
                bool isRestart = timeToRestart.TotalSeconds == 0;

                DateTime now = DateTime.Now;

                if (interval > 0 && now.Day != now.AddHours(interval).Day && ((isRestart && now.AddHours(interval).Hour > 0) || (!isRestart && now.AddHours(interval).Hour >= 0)))
                {
                    DateTime nextDay = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);
                    TimeSpan nextRestart = nextDay - now;
                    _timer.Change(
                        nextRestart - timeToRestart,
                        new TimeSpan(interval, 0, 0)
                    );
                }
            }
        }

        private void ExecuteFunction(string cmd, TimeSpan interval)
        {
            if (_scheduler.IsConnected())
            {
                Logger.Info($"Sending command {cmd}");
                _scheduler.SendCommand(cmd);
            }

            if (interval.TotalSeconds == 0)
            {
                _timer.Dispose();
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
