using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.SerializationClasses.MissionClasses.EconomyCoreClasses
{
    [XmlRoot("economycore")]
    public class EconomyCoreFile
    {
        [XmlElement("classes")]
        public ClassesItem classes;
        [XmlElement("defaults")]
        public DefaultsItem defaults;
        [XmlElement("ce")]
        public List<CeItem> ceItems;
    }
}
