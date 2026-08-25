using Domain.Scheduler;

namespace Domain.Manager;

public class SchedulerInformation
{
    public int playersCount { get; set; }
    public List<ConnectedPlayer> players { get; set; }
    public string chatLog { get; set; }
}