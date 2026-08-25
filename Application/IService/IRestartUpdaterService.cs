using Application.Handlers;
using Domain.Manager;
using Domain.Profile;

namespace Application.IService;

public interface IRestartUpdaterService
{
    public List<JobTimer> CreateSchedule(bool isOnUpdate, bool onlyRestarts, int interval, Action<string> sendCommand,
        Func<bool> isConnected);
    public List<JobTimer> CreateCustomJobTimers(bool onlyRestarts, int interval, Action<string> sendCommand,
        Func<bool> isConnected, List<CustomMessage> customMessages);
    public DateTime GetNextRestartTime(int interval, DateTime now);
    public DateTime GetNextRestartTimeUpdate(DateTime now);
    public void UpdateExpansionScheduler(Instance config, NotificationSchedulerFile expansionScheduler);
    public bool IsTimeToRestart(int interval);
}