
namespace DayZScheduler.Classes.SerializationClasses.SchedulerClasses
{
    public class JobItem
    {
        public int ID { get; set; }
        public bool IsTimeOfDay { get; set; }
        public Dictionary<string,double> WaitTime { get; set; }
        public Dictionary<string,double> Interval { get; set; }
        public int Loop { get; set; }
        public string Cmd {  get; set; }

        public JobItem(int id, bool timeofday, Dictionary<string,double> start, Dictionary<string,double> runtime, int loop, string cmd)
        {
            this.ID = id;
            this.IsTimeOfDay = timeofday;
            this.WaitTime = start;
            this.Interval = runtime;
            this.Loop = loop;
            this.Cmd = cmd;
        }
    }
}
