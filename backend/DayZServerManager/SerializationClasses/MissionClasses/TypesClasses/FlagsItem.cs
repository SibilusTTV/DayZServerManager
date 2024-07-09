using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.SerializationClasses.MissionClasses.TypesClasses
{
    [XmlRoot("flags")]
    public class FlagsItem
    {
        [XmlAttribute("count_in_cargo")]
        public string count_in_cargo;
        [XmlAttribute("count_in_hoarder")]
        public string count_in_hoarder;
        [XmlAttribute("count_in_map")]
        public string count_in_map;
        [XmlAttribute("count_in_player")]
        public string count_in_player;
        [XmlAttribute("crafted")]
        public string crafted;
        [XmlAttribute("deloot")]
        public string deloot;
    }
}
