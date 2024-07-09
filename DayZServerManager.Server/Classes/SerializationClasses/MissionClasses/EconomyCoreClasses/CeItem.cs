using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EconomyCoreClasses
{
    public class CeItem
    {
        [XmlAttribute("folder")]
        public string folder;
        [XmlElement("file")]
        public List<FileItem> fileItems;
    }
}
