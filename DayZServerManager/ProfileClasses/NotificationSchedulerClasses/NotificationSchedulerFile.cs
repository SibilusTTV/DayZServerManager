using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.ProfileClasses.NotificationSchedulerClasses
{
    internal class NotificationSchedulerFile
    {
        public int m_Version { get; set; }
        public int Enabled { get; set; }
        public int UTC { get; set; }
        public int UseMissionTime { get; set; }
        public List<NotificationItem> Notifications { get; set; }
    }
}
