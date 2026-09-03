using System.Globalization;
using System.Text;
using Application.IRepository;
using Domain.Constants;
using Domain.Manager;
using Domain.Mission.EconomyCore;
using Domain.Mission.Environment;
using Domain.Mission.EventSpawn;
using Domain.Mission.Globals;
using Domain.Mission.Hardline;
using Domain.Mission.RarityFile;
using Domain.Mission.Types;
using Domain.Mission.TypesChanges;
using Domain.Profile;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;

namespace Infrastructure.Repository;

public class ServerRepository : IServerRepository
{
    private readonly ILogger<ServerRepository> _logger;
    private readonly IServiceScope _serverScope;

    public ServerRepository(ILogger<ServerRepository> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _serverScope = scopeFactory.CreateScope();
    }

    public void CreateFoldersAndFiles(string serverFolderPath, string profileName, string battlEyeFolderPath)
    {
        if (!Directory.Exists(serverFolderPath))
        {
            Directory.CreateDirectory(serverFolderPath);
        }

        if (!File.Exists(Path.Combine(serverFolderPath, Files.BanFileName)))
        {
            File.Create(Path.Combine(serverFolderPath, Files.BanFileName));
        }

        if (!Directory.Exists(Path.Combine(serverFolderPath, profileName)))
        {
            Directory.CreateDirectory(Path.Combine(serverFolderPath, profileName));
        }

        if (!Directory.Exists(battlEyeFolderPath))
        {
            Directory.CreateDirectory(battlEyeFolderPath);
        }

        if (!File.Exists(Path.Combine(battlEyeFolderPath, Files.BattlEyeBansFileName)))
        {
            using (var fs = File.Create(Path.Combine(battlEyeFolderPath, Files.BattlEyeBansFileName)))
            {

            }
        }
    }

    public void UpdateBeConfigs(string battlEyeFolderPath, string rConPassword, int rConPort)
    {
        var beConfigFiles = FileSystem.GetFiles(battlEyeFolderPath).ToList().FindAll(beFile => Path.GetExtension(beFile) == ".cfg" && Path.GetFileNameWithoutExtension(beFile).Contains(Path.GetFileNameWithoutExtension(Files.BattlEyeConfigFileName)));
        if (beConfigFiles.Count > 0)
        {
            foreach (var beConfigFile in beConfigFiles)
            {
                UpdateBeConfigFile(beConfigFile, rConPassword, rConPort);
            }
        }
        else
        {
            var beConfigPath = Path.Combine(battlEyeFolderPath, Files.BattlEyeConfigFileName);
            using (var fs = File.Create(beConfigPath))
            {
        
            }
            UpdateBeConfigFile(beConfigPath, rConPassword, rConPort);
        }
    }
    
    public string GetAdminLog(string serverFolderPath, string profileName)
    {
        try
        {
            var adminLogPath = OperatingSystem.IsWindows() ? 
                Path.Combine(serverFolderPath, profileName, Files.AdminLogX64Name) : 
                Path.Combine(serverFolderPath, profileName, Files.AdminLogName);

            if (!File.Exists(adminLogPath))
            {
                adminLogPath = Directory.GetFiles(Path.Combine(serverFolderPath, profileName)).Order()
                    .LastOrDefault(x => Path.GetExtension(x) == ".ADM");

                if (adminLogPath == null || !File.Exists(adminLogPath)) return "";
            }
            
            using (var fs = new FileStream(adminLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs, Encoding.Default))
                {
                    return sr.ReadToEnd();
                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when getting admin log");
            return string.Empty;
        }
    }

    public List<long> CheckForUpdates(List<Mod> mods, string  serverFolderName, out bool missionNeedsUpdating, out bool updatedServer)
    {
        missionNeedsUpdating = false;
        updatedServer = CheckForUpdatedServer(serverFolderName);
        var updatedModsIDs = CheckForUpdatedMods(mods, serverFolderName, out missionNeedsUpdating);
        return updatedModsIDs;
    }

    public void MoveServer(string serverFolderName, string profileName, string serverConfigName)
    {
        var serverDeployDirectories = Directory.GetDirectories(Folders.DeployFolderName).ToList();
        var serverDeployFiles = Directory.GetFiles(Folders.DeployFolderName).ToList();

        var filteredDirectories = serverDeployDirectories.FindAll(x => Path.GetFileName(x) != profileName && Path.GetFileName(x) != Folders.BattleyeFolderName);
        var filteredFiles = serverDeployFiles.FindAll(x => !Path.GetFileName(x).Equals(Files.BansFileName, StringComparison.CurrentCultureIgnoreCase) && 
                                                           !Path.GetFileName(x).Equals(Files.BanFileName, StringComparison.CurrentCultureIgnoreCase) && 
                                                           Path.GetFileName(x) != serverConfigName && 
                                                           !Path.GetFileName(x).Equals(Files.WhitelistFileName, StringComparison.CurrentCultureIgnoreCase) && 
                                                           !Path.GetFileName(x).Equals(Files.DayZSettingsFileName, StringComparison.CurrentCultureIgnoreCase));

        foreach (var dir in serverDeployDirectories)
        {
            try
            {
                FileSystem.CopyDirectory(dir, Path.Combine(serverFolderName, Path.GetFileName(dir)), true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying a directory");
            }
        }

        foreach (var file in filteredFiles)
        {
            try
            {
                File.Copy(file, Path.Combine(serverFolderName, Path.GetFileName(file)), true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying a file");
            }
        }
    }

    public void MoveMods(List<Mod> mods, List<long> updatedModIds, string serverFolderName)
    {
        foreach (var key in updatedModIds)
        {
            try
            {
                var mod = mods.Find(x => x.workshopID == key);
                
                if (mod == null) continue;
                
                var steamModPath = Path.Combine(Folders.ModsFolderName, Folders.WorkshopFolderPath, mod.workshopID.ToString());
                var serverModPath = Path.Combine(serverFolderName, mod.name);

                _logger.LogInformation($"Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                
                if (!Directory.Exists(steamModPath)) continue;
                
                FileSystem.CopyDirectory(steamModPath, serverModPath, true);

                string serverKeysPath = GetKeysFolder(serverFolderName);
                string modKeysPath = GetKeysFolder(serverModPath);

                if (modKeysPath != string.Empty && serverKeysPath != string.Empty && Directory.Exists(modKeysPath) && Directory.Exists(serverKeysPath))
                {
                    FileSystem.CopyDirectory(modKeysPath, serverKeysPath, true);
                }
                _logger.LogInformation($"Mod was moved to {mod.name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving mods");
            }
        }
    }

    public void BackupServerData(bool deleteBackups, string backupPath, string profileName, string missionName, int maxKeepTime, string serverFolderName)
    {
        MakeBackup(backupPath, profileName, missionName, serverFolderName);
        if (deleteBackups)
        {
            DeleteOldBackups(backupPath, maxKeepTime);
        }
    }
    
    private void MakeBackup(string backupPath, string profileName, string missionName, string serverFolderName)
    {
        try
        {
            if (!FileSystem.DirectoryExists(backupPath))
            {
                FileSystem.CreateDirectory(backupPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.ToString());
        }

        try
        {
            _logger.LogInformation($"Backing up the server data and moving all the logs!");
            var newestBackupPath = Path.Combine(backupPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            var dataPath = Path.Combine(serverFolderName, Folders.MpmissionsFolderName, missionName, "storage_1");
            var profilePath = Path.Combine(serverFolderName, profileName);
            if (FileSystem.DirectoryExists(dataPath))
            {
                FileSystem.CopyDirectory(dataPath, Path.Combine(newestBackupPath, "data"));
            }
            if (FileSystem.DirectoryExists(profilePath))
            {
                var filePaths = FileSystem.GetFiles(profilePath).ToArray();
                foreach (var filePath in filePaths)
                {
                    if (Path.GetExtension(filePath) == ".ADM" || Path.GetExtension(filePath) == ".RPT" || Path.GetExtension(filePath) == ".log" || Path.GetExtension(filePath) == ".mdmp")
                    {
                        FileSystem.MoveFile(filePath, Path.Combine(newestBackupPath, "logs", Path.GetFileName(filePath)));
                    }
                }
            }
            _logger.LogInformation($"Server backup and moving of the logs done");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating backups");
        }
    }

    private void DeleteOldBackups(string backupPath, int maxKeepTime)
    {
        try
        {
            if (!FileSystem.DirectoryExists(backupPath)) return;
            
            var dateTreshold = DateTime.Now.AddDays(-maxKeepTime);
            var backupFolders = FileSystem.GetDirectories(backupPath).ToList();
            foreach (var folder in backupFolders)
            {
                DateTime folderDate;
                var isValidDate = DateTime.TryParseExact(Path.GetFileName(folder), "yyyy-MM-dd HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out folderDate);
                if (isValidDate && folderDate < dateTreshold)
                {
                    FileSystem.DeleteDirectory(folder, DeleteDirectoryOption.DeleteAllContents);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting old backups");
        }
    }
    
    private void UpdateBeConfigFile(string beConfigFile, string rConPassword, int rConPort)
    {
        try
        {
            var beConfig = $"RConPassword {rConPassword}";
            beConfig += $"{Environment.NewLine}RConPort {rConPort}";
            beConfig += $"{Environment.NewLine}RestrictRCon 0";

            using var writer = new StreamWriter(beConfigFile);
            writer.Write(beConfig);
            writer.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when updating be config");
        }
    }
    
    private bool CheckForUpdatedServer(string serverFolderPath)
    {
        try
        {
            var dateBeforeUpdate = File.Exists(Path.Combine(serverFolderPath, Files.ServerExecutableFileName)) ? 
                File.GetLastWriteTimeUtc(Path.Combine(serverFolderPath, Files.ServerExecutableFileName)) : 
                DateTime.MinValue;

            var dateAfterUpdate = File.Exists(Path.Combine(Folders.DeployFolderName, Files.ServerExecutableFileName)) ? 
                File.GetLastWriteTimeUtc(Path.Combine(Folders.DeployFolderName, Files.ServerExecutableFileName)) : 
                DateTime.MaxValue;

            if (dateBeforeUpdate < dateAfterUpdate)
            {
                _logger.LogInformation("DayZ Server updated");
                return true;
            }
            else
            {
                _logger.LogInformation("Server was already up-to-date");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when checking updated status of the server");
            return false;
        }
    }

    private List<long> CheckForUpdatedMods(List<Mod> mods, string serverFolderPath, out bool missionNeedsUpdating)
    {
        List<long> updatedModsIDs = [];
        missionNeedsUpdating = false;
        
        foreach (var mod in mods)
        {
            if (!Directory.Exists(Path.Combine(Folders.ModsFolderName, Folders.WorkshopFolderPath, mod.workshopID.ToString())) || !CompareForChanges(
                    Path.Combine(Folders.ModsFolderName, Folders.WorkshopFolderPath, mod.workshopID.ToString()),
                    Path.Combine(serverFolderPath, mod.name))) continue;
            
            if (!updatedModsIDs.Contains(mod.workshopID))
            {
                _logger.LogInformation($"{mod.name} was updated");
                updatedModsIDs.Add(mod.workshopID);
                if (mod.name.Contains(SteamCmd.ExpansionModSearch, StringComparison.CurrentCultureIgnoreCase))
                {
                    missionNeedsUpdating = true;
                }
            }
        }
        
        return updatedModsIDs;
    }

    private bool CompareForChanges(string steamModPath, string serverModPath)
    {
        var steamModFilePaths = Directory.GetFiles(steamModPath).ToList();
        foreach (var filePath in steamModFilePaths)
        {
            if (CheckFile(filePath, serverModPath))
            {
                return true;
            }
        }

        var steamModDirectoryPaths = Directory.GetDirectories(steamModPath).ToList();
        foreach (var directoryPath in steamModDirectoryPaths)
        {
            if (CheckDirectories(directoryPath, serverModPath))
            {
                return true;
            }
        }

        return false;
    }
    
    private bool CheckDirectories(string steamDirectoryPath, string serverModPath)
    {
        try
        {
            var serverDirectoryPath = Path.Combine(serverModPath, Path.GetFileName(steamDirectoryPath));
            if (Directory.Exists(serverModPath) && Directory.Exists(serverDirectoryPath))
            {
                var steamModFilePaths = Directory.GetFiles(steamDirectoryPath).ToList();
                foreach (var filePath in steamModFilePaths)
                {
                    if (CheckFile(filePath, serverDirectoryPath))
                    {
                        return true;
                    }
                }

                var steamModDirectoryPaths = Directory.GetDirectories(steamDirectoryPath).ToList();
                foreach (var directoryPath in steamModDirectoryPaths)
                {
                    if (CheckDirectories(directoryPath, serverDirectoryPath))
                    {
                        return true;
                    }
                }

                return false;
            }
            else if (Directory.Exists(serverModPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when checking directories");
            return false;
        }
    }

    private bool CheckFile(string steamFilePath, string serverModPath)
    {
        try
        {
            var serverFilePath = Path.Combine(serverModPath, Path.GetFileName(steamFilePath));
            if (File.Exists(steamFilePath) && File.Exists(serverFilePath))
            {
                var steamModChangingDate = File.GetLastWriteTimeUtc(steamFilePath);
                var serverModChangingDate = File.GetLastWriteTimeUtc(serverFilePath);
                return steamModChangingDate > serverModChangingDate;
            }
            else if (File.Exists(steamFilePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when checking file");
            return false;
        }
    }

    public NotificationSchedulerFile UpdateExpansionNotificationFile(string serverFolderName, string profileName)
    {
        if (!Directory.Exists(Path.Combine(serverFolderName, profileName, Folders.ProfileExpansionModFolderName)))
        {
            Directory.CreateDirectory(Path.Combine(serverFolderName, profileName, Folders.ProfileExpansionModFolderName));
        }

        if (!Directory.Exists(Path.Combine(serverFolderName, profileName, Folders.ProfileExpansionModFolderName, Folders.ProfileExpansionSettingsFolderName)))
        {
            Directory.CreateDirectory(Path.Combine(serverFolderName, profileName, Folders.ProfileExpansionModFolderName, Folders.ProfileExpansionSettingsFolderName));
        }

        var jsonSerializer = _serverScope.ServiceProvider.GetService<IJsonSerializerRepository>();
        var notFile = jsonSerializer?.DeserializeJSONFile<NotificationSchedulerFile>(Path.Combine(serverFolderName, profileName, Folders.ProfileExpansionModFolderName, Folders.ProfileExpansionSettingsFolderName, Files.ProfileExpansionNotificationSchedulerSettingsFileName)) ??
                      new NotificationSchedulerFile(1, 1, 0, 0, new List<NotificationItem>());
        jsonSerializer?.SerializeJSONFile(Path.Combine(serverFolderName, profileName, Folders.ProfileExpansionModFolderName, Folders.ProfileExpansionSettingsFolderName, Files.ProfileExpansionNotificationSchedulerSettingsFileName), notFile);
        return notFile;
    }
    
    private string GetKeysFolder(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath)) return string.Empty;
            
            var subFolders = Directory.GetDirectories(folderPath).ToList();
            foreach (var subFolder in subFolders)
            {
                var folderName = Path.GetFileName(subFolder);
                if (folderName.Equals("keys", StringComparison.CurrentCultureIgnoreCase) || 
                    folderName.Equals("key", StringComparison.CurrentCultureIgnoreCase))
                {
                    return subFolder;
                }
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting keys folder");
            return string.Empty;
        }
    }
}