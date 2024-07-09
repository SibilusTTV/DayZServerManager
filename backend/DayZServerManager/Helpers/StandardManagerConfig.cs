using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Helpers
{
    internal static class StandardManagerConfig
    {
        private static string steamUsername = "";
        private static string steamPassword = "";
        private static string serverPath = "Server";
        private static string steamCMDPath = "SteamCMD";
        private static string becPath = "BEC";
        private static string workshopPath = Path.Combine(steamCMDPath, "steamapps", "workshop", "content", "221100");
        private static string backupPath = "Backup";
        private static string missionName = "Expansion.ChernarusPlus";
        private static int instanceId = 1;
        private static string serverConfigName = "serverDZ.cfg";
        private static string profileName = "Profiles";
        private static int port = 2302;
        private static int steamQueryPort = 2305;
        private static int RConPort = 2306;
        private static int cpuCount = 8;
        private static bool noFilePatching = true;
        private static bool doLogs = true;
        private static bool adminLog = true;
        private static bool netLog = true;
        private static bool freezeCheck = true;
        private static int limitFPS = -1;
        private static string vanillaMissionName = "dayzOffline.chernarusplus";
        private static string missionTemplatePath = Path.Combine(serverPath, "mpmissions", "ChernarusTemplate");
        private static string expansionDownloadPath = Path.Combine("Server", "mpmissions", "DayZ-Expansion-Missions");
        private static string mapName = "Chernarus";
        private static bool RestartOnUpdate = true;
        private static int RestartInterval = 4;

        public static string STEAMUSERNAME
        {
            get { return steamUsername; }

        }

        public static string STEAMPASSWORD
        {
            get { return steamPassword; }
        }

        public static string SERVERPATH
        {
            get { return serverPath; }
        }

        public static string STEAMCMDPATH
        {
            get { return steamCMDPath; }
        }

        public static string BECPATH
        {
            get { return becPath; }
        }

        public static string WORKSHOPPATH
        {
            get { return workshopPath; }
        }

        public static string BACKUPPATH
        {
            get { return backupPath; }
        }

        public static string MISSIONNAME
        {
            get { return missionName; }
        }

        public static int INSTANCEID
        {
            get { return instanceId; }
        }

        public static string SERVERCONFIGNAME
        {
            get { return serverConfigName; }
        }

        public static string PROFILENAME
        {
            get { return profileName; }
        }

        public static int PORT
        {
            get { return port; }
        }

        public static int STEAMQUERYPORT
        {
            get { return steamQueryPort; }
        }

        public static int RCONPORT
        {
            get { return RConPort; }
        }

        public static int CPUCOUNT
        {
            get { return cpuCount; }
        }

        public static bool NOFILEPATCHING
        {
            get { return noFilePatching; }
        }

        public static bool DOLOGS
        {
            get { return doLogs; }
        }

        public static bool ADMINLOG
        {
            get { return adminLog; }
        }

        public static bool NETLOG
        {
            get { return netLog; }
        }

        public static bool FREEZECHECK
        {
            get { return freezeCheck; }
        }

        public static int LIMITFPS
        {
            get { return limitFPS; }
        }

        public static string VANILLAMISSIONNAME
        {
            get { return vanillaMissionName; }
        }

        public static string MISSIONTEMPLATEPATH
        {
            get { return missionTemplatePath; }
        }

        public static string EXPANSIONDOWNLOADPATH
        {
            get { return expansionDownloadPath; }
        }

        public static string MAPNAME
        {
            get { return mapName; }
        }

        public static bool RESTARTONUPDATE
        {
            get { return RestartOnUpdate; }
        }

        public static int RESTARTINTERVAL
        {
            get { return RestartInterval; }
        }


    }
}
