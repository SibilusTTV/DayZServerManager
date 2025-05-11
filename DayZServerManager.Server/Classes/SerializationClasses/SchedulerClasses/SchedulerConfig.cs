
using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;

namespace DayZScheduler.Classes.SerializationClasses.SchedulerClasses
{
    public class SchedulerConfig
    {
        public bool UseNickFilter { get; set; }
        public string FilteredNickMsg { get; set; }
        public List<string> BadNames { get; set; }
        public int Timeout { get; set; }

        public SchedulerConfig()
        {
            UseNickFilter = true;
            FilteredNickMsg = "You are using forbidden words in your user name";
            BadNames = new List<string>();
            Timeout = 60;
        }
    }
}
