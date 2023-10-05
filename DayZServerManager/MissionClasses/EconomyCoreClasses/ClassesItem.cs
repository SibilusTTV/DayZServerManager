using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.MissionClasses.EconomyCoreClasses
{
    [XmlRoot("classes")]
    public class ClassesItem
    {
        [XmlElement("rootclass")]
        public List<RootClassItem> rootClassItems;
    }
}
