namespace DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses
{
    public class CustomMessage
    {
        public bool IsTimeOfDay { get; set; }
        public Dictionary<string, double> WaitTime {  get; set; }
        public Dictionary<string, double> Interval { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        public CustomMessage(bool isTimeOfDay, Dictionary<string, double> waitTime, Dictionary<string, double> interval, string title, string message, string icon, string color)
        {
            this.IsTimeOfDay = isTimeOfDay;
            this.WaitTime = waitTime;
            this.Interval = interval;
            this.Title = title;
            this.Message = message;
            this.Icon = icon;
            this.Color = color;
        }
    }
}
