using Domain.Manager;
using Domain.Profile;

namespace Application.IRepository;

public interface IServerRepository
{
    public void UpdateBeConfigs(string battlEyeFolderPath, string rConPassword, int rConPort);
    public void MoveServer(string serverFolderName, string profileName, string serverConfigName);
    public void MoveMods(List<Mod> mods, List<long> updatedModIds, string serverFolderName);
    public NotificationSchedulerFile UpdateExpansionNotificationFile(string serverFolderName, string profileName);

    public void BackupServerData(bool deleteBackups, string backupPath, string profileName, string missionName,
        int maxKeepTime, string serverFolderName);

    public List<long> CheckForUpdates(List<Mod> mods, string serverFolderName, out bool updatedMods,
        out bool missionNeedsUpdating, out bool updatedServer);

    public string GetAdminLog(string serverFolderPath, string profileName);
    public void CreateFoldersAndFiles(string serverFolderPath, string profileName, string battlEyeFolderPath);
}