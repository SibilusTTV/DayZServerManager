namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class BanPlayerProps
    {
        public string guid { get; set; }
        public string name { get; set; }
        public int duration { get; set; }
        public string reason { get; set; }
        public BanPlayerProps(string guid, string name, int duration, string reason)
        {
            this.guid = guid;
            this.name = name;
            this.duration = duration;
            this.reason = reason;
        }
    }
}
