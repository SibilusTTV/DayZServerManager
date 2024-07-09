using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.SerializationClasses.MissionClasses.EventSpawnClasses
{
    [XmlRoot("pos")]
    public class PosItem
    {
        [XmlAttribute("x")]
        public string x;
        [XmlAttribute("y")]
        public string y;
        [XmlAttribute("z")]
        public string z;
        [XmlAttribute("a")]
        public string a;
    }
}
