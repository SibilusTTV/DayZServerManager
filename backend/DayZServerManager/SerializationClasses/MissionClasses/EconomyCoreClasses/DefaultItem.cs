using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.SerializationClasses.MissionClasses.EconomyCoreClasses
{
    [XmlRoot("default")]
    public class DefaultItem
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("value")]
        public string value;
    }
}
