using DayZServerManager.Server.Classes.Helpers.Property;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses
{
    public class ServerConfig
    {
        public List<PropertyValue> Properties { get; set; }

        public ServerConfig()
        {
            Properties = new List<PropertyValue>();
        }

        public void SetDefaultValues()
        {
            Properties = new List<PropertyValue>();

            Properties.Add(new PropertyValue(0, "hostname", DataType.TextShort, "EXAMPLE NAME", "Server name"));
            Properties.Add(new PropertyValue(1, "password", DataType.Text, "", "Password to connect to the server"));
            Properties.Add(new PropertyValue(2, "passwordAdmin", DataType.Text, "", "Password to become a server admin"));
            Properties.Add(new PropertyValue(3, "enableWhitelist", DataType.Number, 0, "Enable/disable whitelist (value 0-1)"));
            Properties.Add(new PropertyValue(4, "disableBanlist", DataType.Boolean, false, "Disables the usage of ban.txt (default: false)"));
            Properties.Add(new PropertyValue(5, "disablePrioritylist", DataType.Boolean, false, "Disables usage of priority.txt (default: false)"));
            Properties.Add(new PropertyValue(6, "maxPlayers", DataType.Number, 60, "Maximum amount of players"));
            Properties.Add(new PropertyValue(7, "verifySignatures", DataType.Number, 2, "Verifies .pbos against .bisign files. (only 2 is supported)"));
            Properties.Add(new PropertyValue(8, "forceSameBuild", DataType.Number, 1, "When enabled, the server will allow the connection only to clients with same the .exe revision as the server (value 0-1)"));
            Properties.Add(new PropertyValue(9, "disableVoN", DataType.Number, 0, "Enable/disable voice over network (value 0-1)"));
            Properties.Add(new PropertyValue(10, "vonCodecQuality", DataType.Number, 20, "Voice over network codec quality, the higher the better (values 0-20)"));
            Properties.Add(new PropertyValue(11, "enableCfgGameplayFile", DataType.Number, 1, "Enables the cfggameplay.json in the mission folder"));
            Properties.Add(new PropertyValue(12, "disable3rdPerson", DataType.Number, 0, "Toggles the 3rd person view for players (value 0-1)"));
            Properties.Add(new PropertyValue(13, "disableCrosshair", DataType.Number, 0, "Toggles the cross-hair (value 0-1)"));
            Properties.Add(new PropertyValue(14, "serverTime", DataType.Text, "SystemTime", "Initial in-game time of the server. \"SystemTime\" means the local time of the machine. Another possibility is to set the time to some value in \"YYYY/MM/DD/HH/MM\" format, e.g \"2015/4/8/17/23\"."));
            Properties.Add(new PropertyValue(15, "serverTimeAcceleration", DataType.Number, 1, "Accelerated Time - The numerical value being a multiplier (0.1-64). Thus, in case it is set to 24, time would move 24 times faster than normal. An entire day would pass in one hour."));
            Properties.Add(new PropertyValue(16, "serverNightTimeAcceleration", DataType.Number, 1, "Accelerated Night Time - The numerical value being a multiplier (0.1-64) and also multiplied by serverTimeAcceleration value. Thus, in case it is set to 4 and serverTimeAcceleration is set to 2, night time would move 8 times faster than normal. An entire night would pass in 3 hours."));
            Properties.Add(new PropertyValue(17, "serverTimePersistent", DataType.Number, 0, "Persistent Time (value 0-1)// The actual server time is saved to storage, so when active, the next server start will use the saved time value."));
            Properties.Add(new PropertyValue(18, "guaranteedUpdates", DataType.Number, 1, "Communication protocol used with game server (use only number 1)"));
            Properties.Add(new PropertyValue(19, "loginQueueConcurrentPlayers", DataType.Number, 5, "The number of players concurrently processed during the login process. Should prevent massive performance drop during connection when a lot of people are connecting at the same time."));
            Properties.Add(new PropertyValue(20, "loginQueueMaxPlayers", DataType.Number, 500, "The maximum number of players that can wait in login queue"));
            Properties.Add(new PropertyValue(21, "instanceId", DataType.Number, 1, "DayZ server instance id, to identify the number of instances per box and their storage folders with persistence files"));
            Properties.Add(new PropertyValue(22, "storageAutoFix", DataType.Number, 1, "Checks if the persistence files are corrupted and replaces corrupted ones with empty ones (value 0-1)"));
            Properties.Add(new PropertyValue(23, "respawnTime", DataType.Number, 5, "Sets the respawn delay (in seconds) before the player is able to get a new character on the server, when the previous one is dead"));
            Properties.Add(new PropertyValue(24, "timeStampFormat", DataType.Text, "Short", "Format for timestamps in the .rpt file (value Full/Short)"));
            Properties.Add(new PropertyValue(25, "logAverageFps", DataType.Number, 10, "Logs the average server FPS (value in seconds), needs to have ''-doLogs'' launch parameter active"));
            Properties.Add(new PropertyValue(26, "logMemory", DataType.Number, 10, "Logs the server memory usage (value in seconds), needs to have the ''-doLogs'' launch parameter active"));
            Properties.Add(new PropertyValue(27, "logPlayers", DataType.Number, 10, "Logs the count of currently connected players (value in seconds), needs to have the ''-doLogs'' launch parameter active"));
            Properties.Add(new PropertyValue(28, "logFile", DataType.Text, "server_console.log", "Saves the server console log to a file in the folder with the other server logs"));
            Properties.Add(new PropertyValue(29, "adminLogPlayerHitsOnly", DataType.Number, 0, "1 - log player hits only / 0 - log all hits ( animals/infected )"));
            Properties.Add(new PropertyValue(30, "adminLogPlacement", DataType.Number, 0, "1 - log placement action ( traps, tents )"));
            Properties.Add(new PropertyValue(31, "adminLogBuildActions", DataType.Number, 0, "1 - log basebuilding actions ( build, dismantle, destroy )"));
            Properties.Add(new PropertyValue(32, "adminLogPlayerList", DataType.Number, 0, "1 - log periodic player list with position every 5 minutes"));
            Properties.Add(new PropertyValue(33, "disableMultiAccountMitigation", DataType.Boolean, false, "disables multi account mitigation on consoles when true (default: false)"));
            Properties.Add(new PropertyValue(34, "enableDebugMonitor", DataType.Number, 1, "shows info about the character using a debug window in a corner of the screen (value 0-1)"));
            Properties.Add(new PropertyValue(35, "steamQueryPort", DataType.Number, 2305, "defines Steam query port, should fix the issue with server not being visible in client server browser"));
            Properties.Add(new PropertyValue(36, "allowFilePatching", DataType.Number, 1, "if set to 1 it will enable connection of clients with \"-filePatching\" launch parameter enabled"));
            Properties.Add(new PropertyValue(37, "simulatedPlayersBatch", DataType.Number, 20, "Set limit of how much players can be simulated per frame (for server performance gain)"));
            Properties.Add(new PropertyValue(38, "multithreadedReplication", DataType.Number, 1, "enables multi-threaded processing of server's replication system. Number of worker threads is derived by settings of jobsystem in dayzSettings.xml by \"maxcores\" and \"reservedcores\" parameters (value 0-1)"));
            Properties.Add(new PropertyValue(39, "speedhackDetection", DataType.Number, 1, "enable speedhack detection, values 1-10 (1 strict, 10 benevolent, can be float)"));
            Properties.Add(new PropertyValue(40, "networkRangeClose", DataType.Number, 20, "network bubble distance for spawn of close objects with items in them (f.i. backpacks), set in meters, default value if not set is 20"));
            Properties.Add(new PropertyValue(41, "networkRangeNear", DataType.Number, 150, "network bubble distance for spawn (despawn +10%) of near inventory items objects, set in meters, default value if not set is 150"));
            Properties.Add(new PropertyValue(42, "networkRangeFar", DataType.Number, 1000, "network bubble distance for spawn (despawn +10%) of far objects (other than inventory items), set in meters, default value if not set is 1000"));
            Properties.Add(new PropertyValue(43, "networkRangeDistantEffect", DataType.Number, 4000, "network bubble distance for spawn of effects (currently only sound effects), set in meters, default value if not set is 4000"));
            Properties.Add(new PropertyValue(44, "networkObjectBatchLogSlow", DataType.Number, 5, "Maximum time a bubble can take to iterate in seconds before it is logged to the console"));
            Properties.Add(new PropertyValue(45, "networkObjectBatchEnforceBandwidthLimits", DataType.Number, 1, "Enables a limiter for object creation based on bandwidth statistics"));
            Properties.Add(new PropertyValue(46, "networkObjectBatchUseEstimatedBandwidth", DataType.Number, 0, "Switch between the method behind finding the bandwidth usage of a connection. If set to 0, it will use the total of the actual data sent since the last server frame, and if set to 1, it will use a crude estimation"));
            Properties.Add(new PropertyValue(47, "networkObjectBatchUseDynamicMaximumBandwidth", DataType.Number, 1, "Determines if the bandwidth limit should be a factor of the maximum bandwidth that can be sent or a hard limit. The maximum bandwidth that can be sent fluctuates depending on demand in the system."));
            Properties.Add(new PropertyValue(48, "networkObjectBatchBandwidthLimit", DataType.Decimal, 0.8f, "The actual limit, could be a [0,1] value or a [1,inf] value depending on networkObjectBatchUseDynamicMaximumBandwidth. See above"));
            Properties.Add(new PropertyValue(49, "networkObjectBatchCompute", DataType.Number, 1000, "Number of objects in the create/destroy lists that are checked in a single server frame"));
            Properties.Add(new PropertyValue(50, "networkObjectBatchSendCreate", DataType.Number, 10, "Maximum number of objects that can be sent for creation"));
            Properties.Add(new PropertyValue(51, "networkObjectBatchSendDelete", DataType.Number, 10, "Maximum number of objects that can be sent for deletion"));
            Properties.Add(new PropertyValue(52, "defaultVisibility", DataType.Number, 1375, "highest terrain render distance on server (if higher than \"viewDistance=\" in DayZ client profile, clientside parameter applies)"));
            Properties.Add(new PropertyValue(53, "defaultObjectViewDistance", DataType.Number, 1375, "highest object render distance on server (if higher than \"preferredObjectViewDistance=\" in DayZ client profile, clientside parameter applies)"));
            Properties.Add(new PropertyValue(54, "lightingConfig", DataType.Number, 0, "0 for brighter night, 1 for darker night"));
            Properties.Add(new PropertyValue(55, "disablePersonalLight", DataType.Number, 1, "disables personal light for all clients connected to server"));
            Properties.Add(new PropertyValue(56, "disableBaseDamage", DataType.Number, 0, "set to 1 to disable damage/destruction of fence and watchtower"));
            Properties.Add(new PropertyValue(57, "disableContainerDamage", DataType.Number, 0, "set to 1 to disable damage/destruction of tents, barrels, wooden crate and seachest"));
            Properties.Add(new PropertyValue(58, "disableRespawnDialog", DataType.Number, 0, "set to 1 to disable the respawn dialog (new characters will be spawning as random)"));
            Properties.Add(new PropertyValue(59, "pingWarning", DataType.Number, 200, "set to define the ping value from which the initial yellow ping warning is triggered (value in milliseconds)"));
            Properties.Add(new PropertyValue(60, "pingCritical", DataType.Number, 250, "set to define the ping value from which the red ping warning is triggered (value in milliseconds)"));
            Properties.Add(new PropertyValue(61, "MaxPing", DataType.Number, 300, "set to define the ping value from which a player is kicked from the server (value in milliseconds)"));
            Properties.Add(new PropertyValue(62, "serverFpsWarning", DataType.Number, 15, "set to define the server fps value under which the initial server fps warning is triggered (minimum value is 11)"));
            Properties.Add(new PropertyValue(63, "shardId", DataType.Text, "123abc", "Six alphanumeric characters for Private server"));
            Properties.Add(new PropertyValue(64, "description", DataType.TextLong, "Test Server", "Description of the server. Gets displayed to users in client server browser."));
            Properties.Add(new PropertyValue(65, "steamProtocolMaxDataSize", DataType.Number, 4096, "How big the data size of the protocol can be. If you have trouble with people that get kicked, decrease the size."));
            Properties.Add(new PropertyValue(66, "motdInterval", DataType.Number, 1, "Time interval (in seconds) between each message"));
            Properties.Add(new PropertyValue(67, "motd[]", DataType.Array, new List<string> { "line1", "line2" }, "Message of the day displayed in the in-game chat"));
            Properties.Add(new PropertyValue(68, "template", DataType.Text, "dayzOffline.chernarusplus", "Mission to load on server startup. <MissionName>.<TerrainName>"));
        }

        public PropertyValue? GetPropertyValue(string key)
        {
            return Properties.Find(x => x.PropertyName == key);
        }

        public string GetPropertyValueAsString(string key)
        {
            PropertyValue? propertyValue = this.GetPropertyValue(key);
            if (propertyValue != null)
            {
                return (string)propertyValue.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        public bool GetPropertyValueAsBool(string key)
        {
            PropertyValue? propertyValue = this.GetPropertyValue(key);
            if (propertyValue != null)
            {
                return (bool)propertyValue.Value;
            }
            else
            {
                return false;
            }
        }

        public int GetPropertyValueAsInt(string key)
        {
            PropertyValue? propertyValue = this.GetPropertyValue(key);
            if (propertyValue != null)
            {
                return (int)propertyValue.Value;
            }
            else
            {
                return 0;
            }
        }

        public float GetPropertyValueAsFloat(string key)
        {
            PropertyValue? propertyValue = this.GetPropertyValue(key);
            if (propertyValue != null)
            {
                return (float)propertyValue.Value;
            }
            else
            {
                return 0.0f;
            }
        }

        public List<T> GetPropertyValueAsArray<T>(string key)
        {
            PropertyValue? propertyValue = this.GetPropertyValue(key);
            if (propertyValue != null)
            {
                return (List<T>)propertyValue.Value;
            }
            else
            {
                return new List<T>();
            }
        }

        public int GetNextID()
        {
            int i;
            for (i = 0; i < Properties.Count; i++)
            {
                if (Properties.Find(x => x.id == i) == null)
                {
                    return i;
                }
            }
            return i++;
        }
    }
}
