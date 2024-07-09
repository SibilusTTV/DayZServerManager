using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses
{
    [XmlRoot("value")]
    public class ValueItem
    {
        [XmlAttribute("name")]
        public string name;
    }
}
