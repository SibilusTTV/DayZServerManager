namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses
{
    public class WhitelistPlayerProps
    {
        public string guid {  get; set; }
        public string name { get; set; }

        public WhitelistPlayerProps(string guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }
    }
}
