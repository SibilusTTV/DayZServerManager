using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.MissionClasses.GlobalsClasses
{
    [XmlRoot("var")]
    public class VarItem
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("type")]
        public string type;
        [XmlAttribute("value")]
        public string value;
    }
}
