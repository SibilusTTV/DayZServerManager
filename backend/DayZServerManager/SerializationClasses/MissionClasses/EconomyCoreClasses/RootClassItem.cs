using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.SerializationClasses.MissionClasses.EconomyCoreClasses
{
    [XmlRoot("rootclass")]
    public class RootClassItem
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("act")]
        public string act;
        [XmlAttribute("reportMemoryLOD")]
        public string reportMemoryLOD;
    }
}
