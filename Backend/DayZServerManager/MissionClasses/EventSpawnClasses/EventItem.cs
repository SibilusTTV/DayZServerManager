using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.MissionClasses.EventSpawnClasses
{
    [XmlRoot("event")]
    public class EventItem
    {
        [XmlAttribute("name")]
        public string name;
        [XmlElement("pos")]
        public List<PosItem> positions;
    }
}
