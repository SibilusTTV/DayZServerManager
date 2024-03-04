using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.MissionClasses.EventSpawnClasses
{
    [XmlRoot("eventposdef")]
    public class EventSpawnsFile
    {
        [XmlElement("event")]
        public List<EventItem> eventItems;
    }
}
