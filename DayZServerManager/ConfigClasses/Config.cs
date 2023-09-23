using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.ConfigClasses
{
    public class Config
    {
        public string steamUsername { get; set; }
        public string steamPassword { get; set; }
        public string serverPath { get; set; }
        public string steamCMDPath { get; set; }
        public string becPath { get; set; }
        public string workshopPath { get; set; }
        public string backupPath { get; set; }
        public string missionName { get; set; }
        public List<Mod> clientMods { get; set; }
        public List<Mod> serverMods { get; set; }
    }
}
