using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.BecClasses
{
    [XmlRoot(ElementName = "Scheduler")]
    [XmlType("Scheduler")]
    public class SchedulerFile
    {
        [XmlElement(ElementName = "job")]
        public List<JobItem> JobItems;
    }
}
