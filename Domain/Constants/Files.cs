namespace Domain.Constants;

public static class Files
{
    public const string ManagerConfigFileName = "config.db";
    public const string SteamCmdTarFileName = "steamcmd.tar";
    public const string BattlEyeBansFileName = "bans.txt";
    public const string SchedulerConfigFileName = "config.json";
    public const string PlayerDatabaseFileName = "players_db.json";
    public const string ManagerLogFileName = "manager.log";
    public const string MissionExpansionTypesFileName = "expansion_types.xml";
    public const string MissionExampleTypesFileName = "exampleTypesFile.xml";
    public const string MissionTypesFileName = "types.xml";
    public const string MissionGlobalsFileName = "globals.xml";
    public const string MissionEconomyCoreFileName = "cfgeconomycore.xml";
    public const string MissionEventSpawnsFileName = "cfgeventspawns.xml";
    public const string MissionEnvironmentsFileName = "cfgenvironment.xml";
    public const string MissionCustomFilesRaritiesFileName = "customFilesRarities.json";
    public const string MissionExpansionRaritiesFileName = "expansionRarities.json";
    public const string MissionVanillaRaritiesFileName = "vanillaRarities.json";
    public const string MissionExpansionTypesChangesFileName = "expansionTypesChanges.json";
    public const string MissionVanillaTypesChangesFileName = "vanillaTypesChanges.json";
    public const string MissionInitFileName = "init.c";
    public const string ProfileExpansionNotificationSchedulerSettingsFileName = "NotificationSchedulerSettings.json";
    public const string BansFileName = "bans.txt";
    public const string BanFileName = "ban.txt";
    public const string WhitelistFileName = "whitelist.txt";
    public const string DayZSettingsFileName = "dayzsetting.xml";
    public const string MissionExpansionHardlineSettingsFileName = "HardlineSettings.json";
    public const string AdminLogName = "DayZServer.ADM";
    public const string AdminLogX64Name = "DayZServer_x64.ADM";
    public const string SteamCmdConsoleLogFileName = "console_log.txt";
    public static readonly string ServerExecutableFileName = OperatingSystem.IsWindows() ? "DayZServer_x64.exe" : "DayZServer";
    public static readonly string SteamCmdExecutableFileName = OperatingSystem.IsWindows() ? "steamcmd.exe" : "steamcmd.sh";
    public static readonly string SteamCmdZipName = OperatingSystem.IsWindows() ? "steamcmd.zip" : "steamcmd_linux.tar.gz";
    public static readonly string BattlEyeConfigFileName = OperatingSystem.IsWindows() ? "BEServer_x64.cfg" : "beserver_x64.cfg";
    public static readonly string SchedulerZipFileName = OperatingSystem.IsWindows() ? "windows.zip" : "linux.zip";
    public static readonly string SchedulerExecutableFileName = OperatingSystem.IsWindows() ? "DayZScheduler.exe" : "DayZScheduler";

    public static List<string> LogExtensions = new()
    {
        ".ADM",
        ".RPT",
        ".log",
        ".mdmp"
    };
}