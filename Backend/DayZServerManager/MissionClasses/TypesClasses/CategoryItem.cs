using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.MissionClasses.TypesClasses
{
    [XmlRoot("Category")]
    public class CategoryItem
    {
        [XmlAttribute("name")]
        public string name;
    }
}
