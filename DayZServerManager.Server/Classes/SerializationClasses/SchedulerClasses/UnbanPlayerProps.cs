namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses
{
    public class UnbanPlayerProps
    {
        public string guid { get; set; }
        public string name { get; set; }
        public UnbanPlayerProps(string guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }
    }
}
