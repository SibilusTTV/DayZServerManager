using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.BecClasses
{
    [XmlRoot("Scheduler")]
    public class SchedulerFile
    {
        [XmlElement("job")]
        public List<JobItem> JobItems;
    }
}
