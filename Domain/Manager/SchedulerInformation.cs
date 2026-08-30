using Domain.Scheduler;

namespace Domain.Manager;

public class SchedulerInformation
{
    public int PlayersCount { get; set; }
    public List<ConnectedPlayer> Players { get; set; }
    public string ChatLog { get; set; }
    public string AdminLog { get; set; }
}