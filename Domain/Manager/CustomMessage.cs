using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Manager;

public class CustomMessage
{
    public string Id { get; set; }
    public bool IsTimeOfDay { get; set; }
    public TimeSpan WaitTime { get; set; }
    public TimeSpan Interval { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; }

    public CustomMessage()
    {
        
    }

    public CustomMessage(bool isTimeOfDay, TimeSpan waitTime, TimeSpan interval, string title, string message, string icon, string color)
    {
        Id = Guid.NewGuid().ToString().ToLower();
        IsTimeOfDay = isTimeOfDay;
        WaitTime = waitTime;
        Interval = interval;
        Title = title;
        Message = message;
        Icon = icon;
        Color = color;
    }
}