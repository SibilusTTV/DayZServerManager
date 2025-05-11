namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses
{
    public class KickPlayerProps
    {
        public string guid { get; set; }
        public string name { get; set; }
        public string reason { get; set; }
        public KickPlayerProps(string guid, string name, string reason)
        {
            this.guid = guid;
            this.name = name;
            this.reason = reason;
        }
    }
}
