namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses
{
    public class RemovePlayerInput
    {
        public int id { get; set; }
        public string name { get; set; }
        public int duration { get; set; }
        public string reason { get; set; }
        public RemovePlayerInput(int id, string name, int duration, string reason)
        {
            this.id = id;
            this.name = name;
            this.duration = duration;
            this.reason = reason;
        }
    }
}
