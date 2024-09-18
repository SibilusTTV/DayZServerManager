namespace DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses
{
    public class CustomMessage
    {
        public int Id { get; set; }
        public bool IsTimeOfDay { get; set; }
        public Dictionary<string, double> WaitTime { get; set; }
        public Dictionary<string, double> Interval { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        public CustomMessage(int id, bool isTimeOfDay, Dictionary<string, double> waitTime, Dictionary<string, double> interval, string title, string message, string icon, string color)
        {
            Id = id;
            IsTimeOfDay = isTimeOfDay;
            WaitTime = waitTime;
            Interval = interval;
            Title = title;
            Message = message;
            Icon = icon;
            Color = color;
        }
    }
}
