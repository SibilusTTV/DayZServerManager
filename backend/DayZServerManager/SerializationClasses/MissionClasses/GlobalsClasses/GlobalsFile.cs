using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.SerializationClasses.MissionClasses.GlobalsClasses
{
    [XmlRoot("variables")]
    public class GlobalsFile
    {
        [XmlElement("var")]
        public VarItem[] varItems;
    }
}
