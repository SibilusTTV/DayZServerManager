using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses
{
    [XmlRoot("file")]
    public class FileItem
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("type")]
        public string type;
    }
}
