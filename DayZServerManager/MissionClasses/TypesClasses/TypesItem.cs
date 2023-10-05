using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.MissionClasses.TypesClasses
{
    [XmlRoot("type")]
    public class TypesItem
    {
        [XmlAttribute("name")]
        public string name;
        [XmlElement("nominal")]
        public int nominal;
        [XmlElement("lifetime")]
        public long lifetime;
        [XmlElement("restock")]
        public int restock;
        [XmlElement("min")]
        public int min;
        [XmlElement("quantmin")]
        public int quantmin;
        [XmlElement("quantmax")]
        public int quantmax;
        [XmlElement("cost")]
        public int cost;
        [XmlElement("flags")]
        public FlagsItem flags;
        [XmlElement("category")]
        public CategoryItem category;
        [XmlElement("usage")]
        public List<UsageItem> usage;
        [XmlElement("value")]
        public List<ValueItem> value;
    }
}
