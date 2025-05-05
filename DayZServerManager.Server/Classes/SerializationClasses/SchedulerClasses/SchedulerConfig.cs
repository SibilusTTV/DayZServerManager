
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;

namespace DayZScheduler.Classes.SerializationClasses.SchedulerClasses
{
    public class SchedulerConfig
    {
        public string BannedMsg { get; set; }
        public bool UseWhiteList { get; set; }
        public string WhiteListFile { get; set; }
        public string WhiteListKickMsg { get; set; }
        public bool UseNickFilter { get; set; }
        public string NickFilterFile { get; set; }
        public string FilteredNickMsg { get; set; }
        public int Timeout { get; set; }
        public int ConnectTimeout { get; set; }

        public SchedulerConfig()
        {
            BannedMsg = "You are banned! Don't come back!";
            UseWhiteList = false;
            WhiteListFile = "white-list.txt";
            WhiteListKickMsg = "You are not whitelisted on this server!";
            UseNickFilter = true;
            NickFilterFile = "bad-names.txt";
            FilteredNickMsg = "You are using forbidden words in your user name";
            Timeout = 60;
            ConnectTimeout = 10;
        }
    }
}
