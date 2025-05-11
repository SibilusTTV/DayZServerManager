namespace DayZServerManager.Server.Controllers
{
    public class UnwhitelistPlayerProps
    {
        public string guid { get; set; }
        public string name { get; set; }

        public UnwhitelistPlayerProps(string guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }
    }
}
