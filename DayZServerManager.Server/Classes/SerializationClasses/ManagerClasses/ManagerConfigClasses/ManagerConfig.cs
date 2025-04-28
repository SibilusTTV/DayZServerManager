using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses
{
    public class ManagerConfig
    {
        public int managerPort { get; set; }
        public string steamUsername { get; set; }
        public string steamPassword { get; set; }
        public string backupPath { get; set; }
        public string missionName { get; set; }
        public string vanillaMissionName { get; set; }
        public string missionTemplateName { get; set; }
        public int instanceId { get; set; }
        public string serverConfigName { get; set; }
        public string profileName { get; set; }
        public int serverPort { get; set; }
        public int steamQueryPort { get; set; }
        public int RConPort { get; set; }
        public string RConPassword { get; set; }
        public int cpuCount { get; set; }
        public bool noFilePatching { get; set; }
        public bool doLogs { get; set; }
        public bool adminLog { get; set; }
        public bool freezeCheck { get; set; }
        public bool netLog { get; set; }
        public int limitFPS { get; set; }
        public string mapName { get; set; }
        public bool restartOnUpdate { get; set; }
        public int restartInterval { get; set; }
        public bool autoStartServer { get; set; }
        public bool makeBackups { get; set; }
        public bool deleteBackups { get; set; }
        public int maxKeepTime { get; set; }
        public List<Mod> clientMods { get; set; }
        public List<Mod> serverMods { get; set; }
        public List<CustomMessage> customMessages { get; set; }

        public ManagerConfig()
        {
            managerPort = 5172;
            steamUsername = "";
            steamPassword = "";
            backupPath = "Backup";
            missionName = "Expansion.ChernarusPlus";
            instanceId = 1;
            serverConfigName = "serverDZ.cfg";
            profileName = "Profiles";
            serverPort = 2302;
            steamQueryPort = 2305;
            RConPort = 2306;
            RConPassword = "YouRConPassword";
            cpuCount = 8;
            noFilePatching = true;
            doLogs = true;
            adminLog = true;
            netLog = true;
            freezeCheck = true;
            limitFPS = -1;
            vanillaMissionName = "dayzOffline.chernarusplus";
            missionTemplateName = "template.chernarus";
            mapName = "Chernarus";
            restartOnUpdate = true;
            restartInterval = 4;
            autoStartServer = false;
            makeBackups = true;
            deleteBackups = true;
            maxKeepTime = 7;
            clientMods = new List<Mod>();
            serverMods = new List<Mod>();
            Mod mod1 = new Mod();
            mod1.id = 0;
            mod1.workshopID = 1559212036;
            mod1.name = "@CF";
            clientMods.Add(mod1);
            Mod mod2 = new Mod();
            mod2.id = 1;
            mod2.workshopID = 1564026768;
            mod2.name = "@Community-Online-Tools";
            clientMods.Add(mod2);
            customMessages = new List<CustomMessage>();
            customMessages.Add(new CustomMessage(0, false, new Dictionary<string, int> { { "hours", 0 }, { "minutes", 5 }, { "seconds", 0 } }, new Dictionary<string, int> { { "hours", 0 }, { "minutes", 15 }, { "seconds", 0 } }, "Need Help?", "Make sure to join our Discord", "ExclamationMark", ""));
        }
    }
}
