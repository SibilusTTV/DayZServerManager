namespace Domain.Constants;

public static class Folders
{
    public const string LogsFolderName = "logs";
    public const string DeployFolderName = "deploy";
    public const string SteamCmdFolderName = "steamcmd";
    public const string ModsFolderName = "mods";
    public const string SchedulerFolderName = "scheduler";
    public const string ManagerLogsFolderName = "logs";
    public const string PersistenceFolderName = "storage_1";
    public const string BackupDataFolderName = "data";
    public const string BackupLogsFolderName = "logs";
    public const string MissionExpansionCeFolderName = "expansion_ce";
    public const string MissionCustomFilesFolderName = "CustomFiles";
    public const string MissionExampleModFilesFolderName = "ExampleModFiles";
    public const string MissionDbFolderName = "db";
    public const string BackupsFullMissionBackupsFolderName = "FullMissionBackups";
    public const string MissionExpansionFolderName = "expansion";
    public const string MissionExpansionSettingsFolderName = "settings";
    public const string ProfileExpansionSettingsFolderName = "Settings";
    public const string ProfileExpansionModFolderName = "ExpansionMod";
    public const string SteamCmdLogsFolderName = "logs";
    public const string SteamCmdLinux32FolderName = "linux32";
    public const string MpmissionsFolderName = "mpmissions";
    public const string SteamappsFolderName = "steamapps";
    public const string WorkshopFolderName = "workshop";
    public const string ContentFolderName = "content";
    public const string ExpansionDownloadFolderPath = "DayZ-Expansion-Missions";
    public const string PermissionFolderName = "PermissionsFramework";
    public const string PlayersFolderName = "Players";
    public const string RolesFolderName = "Roles";
    public static readonly string BattleyeFolderName = OperatingSystem.IsWindows() ? "BattlEye" : "battleye";
    public static readonly string WorkshopFolderPath = Path.Combine(SteamappsFolderName, WorkshopFolderName, ContentFolderName, SteamCmd.DayZGameBranch.ToString());
    public static readonly string SteamcmdConsoleLogFolderPath = OperatingSystem.IsWindows() ? 
        Path.Combine(SteamCmdFolderName, SteamCmdLogsFolderName) : 
        Path.Combine(SteamCmdFolderName, SteamCmdLinux32FolderName, SteamCmdLogsFolderName);
}