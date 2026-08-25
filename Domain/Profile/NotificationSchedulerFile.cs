namespace Domain.Profile;

public class NotificationSchedulerFile
{
    public int m_Version { get; set; }
    public int Enabled { get; set; }
    public int UTC { get; set; }
    public int UseMissionTime { get; set; }
    public List<NotificationItem> Notifications { get; set; }

    public NotificationSchedulerFile(int m_Version, int Enabled, int UTC, int UseMissionTime, List<NotificationItem> Notifications)
    {
        this.m_Version = m_Version;
        this.Enabled = Enabled;
        this.UTC = UTC;
        this.UseMissionTime = UseMissionTime;
        this.Notifications = Notifications;
    }
}