using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.ProfileClasses.NotificationSchedulerClasses
{
    internal class NotificationItem
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        public NotificationItem() { }

        public NotificationItem(int hour, int minute, int second, string title, string text, string icon, string color)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
            Title = title;
            Text = text;
            Icon = icon;
            Color = color;
        }
    }
}
