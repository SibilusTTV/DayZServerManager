namespace Domain.ServerConfig;

public class ServerConfig
{
    public List<PropertyValue> Properties { get; set; }

    public ServerConfig()
    {
        
    }

    public void SetDefaultValues()
    {
        Properties = new List<PropertyValue>();

        Properties.Add(new PropertyValue("hostname", "EXAMPLE NAME", "Server name"));
        Properties.Add(new PropertyValue("password", "", "Password to connect to the server"));
        Properties.Add(new PropertyValue("passwordAdmin", "", "Password to become a server admin"));
        Properties.Add(new PropertyValue("enableWhitelist", "0", "Enable/disable whitelist (value 0-1)"));
        Properties.Add(new PropertyValue("disableBanlist", "False", "Disables the usage of ban.txt (default: false)"));
        Properties.Add(new PropertyValue("disablePrioritylist", "False", "Disables usage of priority.txt (default: false)"));
        Properties.Add(new PropertyValue("maxPlayers", "60", "Maximum amount of players"));
        Properties.Add(new PropertyValue("verifySignatures", "2", "Verifies .pbos against .bisign files. (only 2 is supported)"));
        Properties.Add(new PropertyValue("forceSameBuild", "1", "When enabled, the server will allow the connection only to clients with same the .exe revision as the server (value 0-1)"));
        Properties.Add(new PropertyValue("disableVoN", "0", "Enable/disable voice over network (value 0-1)"));
        Properties.Add(new PropertyValue("vonCodecQuality", "20", "Voice over network codec quality, the higher the better (values 0-20)"));
        Properties.Add(new PropertyValue("enableCfgGameplayFile", "1", "Enables the cfggameplay.json in the mission folder"));
        Properties.Add(new PropertyValue("disable3rdPerson", "0", "Toggles the 3rd person view for players (value 0-1)"));
        Properties.Add(new PropertyValue("disableCrosshair", "0", "Toggles the cross-hair (value 0-1)"));
        Properties.Add(new PropertyValue("serverTime", "SystemTime", "Initial in-game time of the server. \"SystemTime\" means the local time of the machine. Another possibility is to set the time to some value in \"YYYY/MM/DD/HH/MM\" format, e.g \"2015/4/8/17/23\"."));
        Properties.Add(new PropertyValue("serverTimeAcceleration", "1", "Accelerated Time - The numerical value being a multiplier (0.1-64). Thus, in case it is set to 24, time would move 24 times faster than normal. An entire day would pass in one hour."));
        Properties.Add(new PropertyValue("serverNightTimeAcceleration", "1", "Accelerated Night Time - The numerical value being a multiplier (0.1-64) and also multiplied by serverTimeAcceleration value. Thus, in case it is set to 4 and serverTimeAcceleration is set to 2, night time would move 8 times faster than normal. An entire night would pass in 3 hours."));
        Properties.Add(new PropertyValue("serverTimePersistent", "0", "Persistent Time (value 0-1)// The actual server time is saved to storage, so when active, the next server start will use the saved time value."));
        Properties.Add(new PropertyValue("guaranteedUpdates", "1", "Communication protocol used with game server (use only number 1)"));
        Properties.Add(new PropertyValue("loginQueueConcurrentPlayers", "5", "The number of players concurrently processed during the login process. Should prevent massive performance drop during connection when a lot of people are connecting at the same time."));
        Properties.Add(new PropertyValue("loginQueueMaxPlayers", "500", "The maximum number of players that can wait in login queue"));
        Properties.Add(new PropertyValue("instanceId", "1", "DayZ server instance id, to identify the number of instances per box and their storage folders with persistence files"));
        Properties.Add(new PropertyValue("storageAutoFix", "1", "Checks if the persistence files are corrupted and replaces corrupted ones with empty ones (value 0-1)"));
        Properties.Add(new PropertyValue("respawnTime", "5", "Sets the respawn delay (in seconds) before the player is able to get a new character on the server, when the previous one is dead"));
        Properties.Add(new PropertyValue("timeStampFormat", "Short", "Format for timestamps in the .rpt file (value Full/Short)"));
        Properties.Add(new PropertyValue("logAverageFps", "10", "Logs the average server FPS (value in seconds), needs to have ''-doLogs'' launch parameter active"));
        Properties.Add(new PropertyValue("logMemory", "10", "Logs the server memory usage (value in seconds), needs to have the ''-doLogs'' launch parameter active"));
        Properties.Add(new PropertyValue("logPlayers", "10", "Logs the count of currently connected players (value in seconds), needs to have the ''-doLogs'' launch parameter active"));
        Properties.Add(new PropertyValue("logFile", "server_console.log", "Saves the server console log to a file in the folder with the other server logs"));
        Properties.Add(new PropertyValue("adminLogPlayerHitsOnly", "0", "1 - log player hits only / 0 - log all hits ( animals/infected )"));
        Properties.Add(new PropertyValue("adminLogPlacement", "0", "1 - log placement action ( traps, tents )"));
        Properties.Add(new PropertyValue("adminLogBuildActions", "0", "1 - log basebuilding actions ( build, dismantle, destroy )"));
        Properties.Add(new PropertyValue("adminLogPlayerList", "0", "1 - log periodic player list with position every 5 minutes"));
        Properties.Add(new PropertyValue("disableMultiAccountMitigation", "False", "disables multi account mitigation on consoles when true (default: false)"));
        Properties.Add(new PropertyValue("enableDebugMonitor", "1", "shows info about the character using a debug window in a corner of the screen (value 0-1)"));
        Properties.Add(new PropertyValue("steamPort", "2301", ""));
        Properties.Add(new PropertyValue("steamQueryPort", "2305", "defines Steam query port, should fix the issue with server not being visible in client server browser"));
        Properties.Add(new PropertyValue("allowFilePatching", "1", "if set to 1 it will enable connection of clients with \"-filePatching\" launch parameter enabled"));
        Properties.Add(new PropertyValue("simulatedPlayersBatch", "20", "Set limit of how much players can be simulated per frame (for server performance gain)"));
        Properties.Add(new PropertyValue("multithreadedReplication", "1", "enables multi-threaded processing of server's replication system. Number of worker threads is derived by settings of jobsystem in dayzSettings.xml by \"maxcores\" and \"reservedcores\" parameters (value 0-1)"));
        Properties.Add(new PropertyValue("speedhackDetection", "1", "enable speedhack detection, values 1-10 (1 strict, 10 benevolent, can be float)"));
        Properties.Add(new PropertyValue("networkRangeClose", "20", "network bubble distance for spawn of close objects with items in them (f.i. backpacks), set in meters, default value if not set is 20"));
        Properties.Add(new PropertyValue("networkRangeNear", "150", "network bubble distance for spawn (despawn +10%) of near inventory items objects, set in meters, default value if not set is 150"));
        Properties.Add(new PropertyValue("networkRangeFar", "1000", "network bubble distance for spawn (despawn +10%) of far objects (other than inventory items), set in meters, default value if not set is 1000"));
        Properties.Add(new PropertyValue("networkRangeDistantEffect", "4000", "network bubble distance for spawn of effects (currently only sound effects), set in meters, default value if not set is 4000"));
        Properties.Add(new PropertyValue("networkObjectBatchLogSlow", "5", "Maximum time a bubble can take to iterate in seconds before it is logged to the console"));
        Properties.Add(new PropertyValue("networkObjectBatchEnforceBandwidthLimits", "1", "Enables a limiter for object creation based on bandwidth statistics"));
        Properties.Add(new PropertyValue("networkObjectBatchUseEstimatedBandwidth", "0", "Switch between the method behind finding the bandwidth usage of a connection. If set to 0, it will use the total of the actual data sent since the last server frame, and if set to 1, it will use a crude estimation"));
        Properties.Add(new PropertyValue("networkObjectBatchUseDynamicMaximumBandwidth", "1", "Determines if the bandwidth limit should be a factor of the maximum bandwidth that can be sent or a hard limit. The maximum bandwidth that can be sent fluctuates depending on demand in the system."));
        Properties.Add(new PropertyValue("networkObjectBatchBandwidthLimit", "0.8", "The actual limit, could be a [0,1] value or a [1,inf] value depending on networkObjectBatchUseDynamicMaximumBandwidth. See above"));
        Properties.Add(new PropertyValue("networkObjectBatchCompute", "1000", "Number of objects in the create/destroy lists that are checked in a single server frame"));
        Properties.Add(new PropertyValue("networkObjectBatchSendCreate", "10", "Maximum number of objects that can be sent for creation"));
        Properties.Add(new PropertyValue("networkObjectBatchSendDelete", "10", "Maximum number of objects that can be sent for deletion"));
        Properties.Add(new PropertyValue("defaultVisibility", "1375", "highest terrain render distance on server (if higher than \"viewDistance=\" in DayZ client profile, clientside parameter applies)"));
        Properties.Add(new PropertyValue("defaultObjectViewDistance", "1375", "highest object render distance on server (if higher than \"preferredObjectViewDistance=\" in DayZ client profile, clientside parameter applies)"));
        Properties.Add(new PropertyValue("lightingConfig", "0", "0 for brighter night, 1 for darker night"));
        Properties.Add(new PropertyValue("disablePersonalLight", "1", "disables personal light for all clients connected to server"));
        Properties.Add(new PropertyValue("disableBaseDamage", "0", "set to 1 to disable damage/destruction of fence and watchtower"));
        Properties.Add(new PropertyValue("disableContainerDamage", "0", "set to 1 to disable damage/destruction of tents, barrels, wooden crate and seachest"));
        Properties.Add(new PropertyValue("disableRespawnDialog", "0", "set to 1 to disable the respawn dialog (new characters will be spawning as random)"));
        Properties.Add(new PropertyValue("pingWarning", "200", "set to define the ping value from which the initial yellow ping warning is triggered (value in milliseconds)"));
        Properties.Add(new PropertyValue("pingCritical", "250", "set to define the ping value from which the red ping warning is triggered (value in milliseconds)"));
        Properties.Add(new PropertyValue("MaxPing", "300", "set to define the ping value from which a player is kicked from the server (value in milliseconds)"));
        Properties.Add(new PropertyValue("serverFpsWarning", "15", "set to define the server fps value under which the initial server fps warning is triggered (minimum value is 11)"));
        Properties.Add(new PropertyValue("BattlEye", "1", "Turn on BattlEye"));
        // Properties.Add(new PropertyValue("shardId", "123abc", "Six alphanumeric characters for Private server"));
        Properties.Add(new PropertyValue("description", "Test Server", "Description of the server. Gets displayed to users in client server browser."));
        Properties.Add(new PropertyValue("steamProtocolMaxDataSize", "4096", "How big the data size of the protocol can be. If you have trouble with people that get kicked, decrease the size."));
        Properties.Add(new PropertyValue("motdInterval", "1", "Time interval (in seconds) between each message"));
        Properties.Add(new PropertyValue("motd[]", "{\"line1\", \"line2\"}", "Message of the day displayed in the in-game chat. Needs to be in format {\"line1\",\"line2\",...}"));
        Properties.Add(new PropertyValue("template", "dayzOffline.chernarusplus", "Mission to load on server startup. <MissionName>.<TerrainName>"));
    }

    public PropertyValue? GetPropertyValue(string key)
    {
        return Properties.Find(x => x.PropertyName == key);
    }
}