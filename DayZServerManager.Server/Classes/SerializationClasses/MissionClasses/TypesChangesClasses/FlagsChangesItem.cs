using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses
{
    public class FlagsChangesItem
    {
        public string count_in_cargo { get; set; }
        public string count_in_hoarder { get; set; }
        public string count_in_map { get; set; }
        public string count_in_player { get; set; }
        public string crafted { get; set; }
        public string deloot { get; set; }
    }
}
