using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses
{
    public class ServerConfig
    {
        public string hostname { get; set; }
        public string password { get; set; }
        public string passwordAdmin { get; set; }
        public int enableWhitelist { get; set; }
        public bool disableBanlist { get; set; }
        public bool disablePrioritylist { get; set; }
        public int maxPlayers { get; set; }
        public int verifySignatures { get; set; }
        public int forceSameBuild { get; set; }
        public int disableVoN { get; set; }
        public int vonCodecQuality { get; set; }
        public int enableCfgGameplayFile { get; set; }
        public int disable3rdPerson { get; set; }
        public int disableCrosshair { get; set; }
        public int disablePersonalLight { get; set; }
        public int lightingConfig { get; set; }
        public string serverTime { get; set; }
        public int serverTimeAcceleration { get; set; }
        public int serverNightTimeAcceleration { get; set; }
        public int serverTimePersistent { get; set; }
        public int guaranteedUpdates { get; set; }
        public int loginQueueConcurrentPlayers { get; set; }
        public int loginQueueMaxPlayers { get; set; }
        public int instanceId { get; set; }
        public int storageAutoFix { get; set; }
        public int steamQueryPort { get; set; }
        public int respawnTime { get; set; }
        public string timeStampFormat { get; set; }
        public int logAverageFps { get; set; }
        public int logMemory { get; set; }
        public int logPlayers { get; set; }
        public string logFile { get; set; }
        public int adminLogPlayerHitsOnly { get; set; }
        public int adminLogPlacement { get; set; }
        public int adminLogBuildActions { get; set; }
        public int adminLogPlayerList { get; set; }
        public bool disableMultiAccountMitigation { get; set; }
        public int enableDebugMonitor { get; set; }
        public int allowFilePatching { get; set; }
        public int simulatedPlayersBatch { get; set; }
        public int multithreadedReplication { get; set; }
        public int speedhackDetection { get; set; }
        public int networkRangeClose { get; set; }
        public int networkRangeNear { get; set; }
        public int networkRangeFar { get; set; }
        public int networkRangeDistantEffect { get; set; }
        public int networkObjectBatchSend { get; set; }
        public int networkObjectBatchCompute { get; set; }
        public int defaultVisibility { get; set; }
        public int defaultObjectViewDistance { get; set; }
        public int disableBaseDamage { get; set; }
        public int disableContainerDamage { get; set; }
        public int disableRespawnDialog { get; set; }
        public int pingWarning { get; set; }
        public int pingCritical { get; set; }
        public int MaxPing { get; set; }
        public int serverFpsWarning { get; set; }

        public int motdInterval { get; set; }
        public List<string> motd { get; set; }

        public string template { get; set; }

        public ServerConfig()
        {
            hostname = "EXAMPLE NAME";	    // Server name
            password = "";				    // Password to connect to the server
            passwordAdmin = "";			    // Password to become a server admin
            enableWhitelist = 0;		    // Enable/disable whitelist (value 0-1)
            disableBanlist = false;			// Disables the usage of ban.txt (default: false)
            disablePrioritylist = false;    // Disables usage of priority.txt (default: false)
            maxPlayers = 60;			    // Maximum amount of players
            verifySignatures = 2;		    // Verifies .pbos against .bisign files. (only 2 is supported)
            forceSameBuild = 1;			    // When enabled, the server will allow the connection only to clients with same the .exe revision as the server (value 0-1)
            disableVoN = 0;				    // Enable/disable voice over network (value 0-1)
            vonCodecQuality = 20;           // Voice over network codec quality, the higher the better (values 0-20)
            enableCfgGameplayFile = 1;
            disable3rdPerson = 0;		    // Toggles the 3rd person view for players (value 0-1)
            disableCrosshair = 0;		    // Toggles the cross-hair (value 0-1)
            serverTime = "ServerTime";	    // Initial in-game time of the server. "SystemTime" means the local time of the machine. Another possibility is to set the time to some value in "YYYY/MM/DD/HH/MM" format, e.g "2015/4/8/17/23".
            serverTimeAcceleration = 1;	    // Accelerated Time - The numerical value being a multiplier (0.1-64). Thus, in case it is set to 24, time would move 24 times faster than normal. An entire day would pass in one hour.
            serverNightTimeAcceleration = 1;// Accelerated Nigh Time - The numerical value being a multiplier (0.1-64) and also multiplied by serverTimeAcceleration value.
                                            // Thus, in case it is set to 4 and serverTimeAcceleration is set to 2, night time would move 8 times faster than normal.
                                            // An entire night would pass in 3 hours.
            serverTimePersistent = 0;	    // Persistent Time (value 0-1)// The actual server time is saved to storage, so when active, the next server start will use the saved time value.
            guaranteedUpdates = 1;		    // Communication protocol used with game server (use only number 1)
            loginQueueConcurrentPlayers = 5;// The number of players concurrently processed during the login process. Should prevent massive performance drop during connection when a lot of people are connecting at the same time.
            loginQueueMaxPlayers = 500;     // The maximum number of players that can wait in login queue
            instanceId = 1;				    // DayZ server instance id, to identify the number of instances per box and their storage folders with persistence files
            storageAutoFix = 1;			    // Checks if the persistence files are corrupted and replaces corrupted ones with empty ones (value 0-1)
            respawnTime = 5;			    // Sets the respawn delay (in seconds) before the player is able to get a new character on the server, when the previous one is dead
            timeStampFormat = "Short";		// Format for timestamps in the .rpt file (value Full/Short)
            logAverageFps = 1;				// Logs the average server FPS (value in seconds), needs to have ''-doLogs'' launch parameter active
            logMemory = 1;                  // Logs the server memory usage (value in seconds), needs to have the ''-doLogs'' launch parameter active
            logPlayers = 1;                 // Logs the count of currently connected players (value in seconds), needs to have the ''-doLogs'' launch parameter active
            logFile = "server_console.log"; // Saves the server console log to a file in the folder with the other server logs
            adminLogPlayerHitsOnly = 0;     // 1 - log player hits only / 0 - log all hits ( animals/infected )
            adminLogPlacement = 0;          // 1 - log placement action ( traps, tents )
            adminLogBuildActions = 0;       // 1 - log basebuilding actions ( build, dismantle, destroy )
            adminLogPlayerList = 0;         // 1 - log periodic player list with position every 5 minutes
            disableMultiAccountMitigation = false; // disables multi account mitigation on consoles when true (default: false)
            enableDebugMonitor = 1;         // shows info about the character using a debug window in a corner of the screen (value 0-1)
            steamQueryPort = 2305;          // defines Steam query port, should fix the issue with server not being visible in client server browser
            allowFilePatching = 1;          // if set to 1 it will enable connection of clients with "-filePatching" launch parameter enabled
            simulatedPlayersBatch = 20;     // Set limit of how much players can be simulated per frame (for server performance gain)
            multithreadedReplication = 1;   // enables multi-threaded processing of server's replication system
                                            // number of worker threads is derived by settings of jobsystem in dayzSettings.xml by "maxcores" and "reservedcores" parameters (value 0-1)
            speedhackDetection = 1;         // enable speedhack detection, values 1-10 (1 strict, 10 benevolent, can be float)
            networkRangeClose = 20;         // network bubble distance for spawn of close objects with items in them (f.i. backpacks), set in meters, default value if not set is 20
            networkRangeNear = 150;         // network bubble distance for spawn (despawn +10%) of near inventory items objects, set in meters, default value if not set is 150
            networkRangeFar = 1000;         // network bubble distance for spawn (despawn +10%) of far objects (other than inventory items), set in meters, default value if not set is 1000
            networkRangeDistantEffect = 4000;   // network bubble distance for spawn of effects (currently only sound effects), set in meters, default value if not set is 4000
            networkObjectBatchSend = 10;        // number of objects within a player's network bubble that are sent to be created within a server frame
            networkObjectBatchCompute = 1000;   // number of objects within a player's network bubble that are processed to check if it already exists for the player within a server frame
            defaultVisibility = 1375;           // highest terrain render distance on server (if higher than "viewDistance=" in DayZ client profile, clientside parameter applies)
            defaultObjectViewDistance = 1375;   // highest object render distance on server (if higher than "preferredObjectViewDistance=" in DayZ client profile, clientside parameter applies)
            lightingConfig = 0;             // 0 for brighter night, 1 for darker night
            disablePersonalLight = 1;       // disables personal light for all clients connected to server
            disableBaseDamage = 0;          // set to 1 to disable damage/destruction of fence and watchtower
            disableContainerDamage = 0;     // set to 1 to disable damage/destruction of tents, barrels, wooden crate and seachest
            disableRespawnDialog = 0;       // set to 1 to disable the respawn dialog (new characters will be spawning as random)
            pingWarning = 200;              // set to define the ping value from which the initial yellow ping warning is triggered (value in milliseconds)
            pingCritical = 250;             // set to define the ping value from which the red ping warning is triggered (value in milliseconds)
            MaxPing = 300;                  // set to define the ping value from which a player is kicked from the server (value in milliseconds)
            serverFpsWarning = 15;          // set to define the server fps value under which the initial server fps warning is triggered (minimum value is 11)

            motdInterval = 1;				// Time interval (in seconds) between each message
            motd = new List<string> { "line1", "line2" };    // Message of the day displayed in the in-game chat

            template = "dayzOffline.chernarusplus"; // Mission to load on server startup. <MissionName>.<TerrainName>

        }
    }
}
