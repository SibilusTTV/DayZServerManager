namespace Application.Handlers;

public class JobTimer
{
    private Timer _timer;

    public JobTimer(Action<string> sendCommand, Func<bool> isConnected, string cmd, int interval, TimeSpan restartTimeSpan, TimeSpan timeBeforeRestart)
    {
        DateTime now = DateTime.Now;

        _timer = new Timer((state) => { ExecuteFunction(sendCommand, isConnected, cmd, interval, timeBeforeRestart); },
            null,
            restartTimeSpan - timeBeforeRestart,
            new TimeSpan(interval, 0, 0)
        );
    }

    public JobTimer(Action<string> sendCommand, Func<bool> isConnected, string cmd, bool isTimeOfDay, TimeSpan waitTime, TimeSpan interval)
    {
        DateTime now = DateTime.Now;

        if (isTimeOfDay && interval.TotalSeconds == 0)
        {
            interval = new TimeSpan(24, 0, 0);
        }

        _timer = new Timer((state) => { ExecuteFunction(sendCommand, isConnected, cmd, interval); }, null, waitTime, interval);
    }

    private void ExecuteFunction(Action<string> sendCommand, Func<bool> isConnected, string cmd, int interval, TimeSpan timeToRestart)
    {
        if (isConnected())
        {
            sendCommand(cmd);
        }

        if (interval == 0)
        {
            _timer.Dispose();
        }
        else
        {
            var now = DateTime.Now;
            var nextRestart = now.AddHours(interval).Add(timeToRestart);

            if (interval <= 1 || now.Day == nextRestart.Day ||
                nextRestart is not ({ Hour: 0, Minute: > 55 } or { Hour: 1, Minute: < 5 })) return;
            
            var nextDay = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);
            var newNextRestart = nextDay - now;
            _timer.Change(
                newNextRestart - timeToRestart,
                new TimeSpan(interval, 0, 0)
            );
        }
    }

    private void ExecuteFunction(Action<string> sendCommand, Func<bool> isConnected, string cmd, TimeSpan interval)
    {
        if (isConnected())
        {
            sendCommand(cmd);
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