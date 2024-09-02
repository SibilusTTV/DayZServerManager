using Microsoft.VisualBasic.FileIO;
using System.Globalization;

namespace DayZServerManager.Server.Classes.Helpers
{
    public static class BackupManager
    {
        public static void MakeBackup(string backupPath, string profileName, string missionName)
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
                Manager.WriteToConsole(ex.ToString());
            }

            try
            {
                Manager.WriteToConsole($"Backing up the server data and moving all the logs!");
                string newestBackupPath = Path.Combine(backupPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                string dataPath = Path.Combine(Manager.MISSION_PATH, missionName, "storage_1");
                string profilePath = Path.Combine(Manager.SERVER_PATH, profileName);
                if (FileSystem.DirectoryExists(dataPath))
                {
                    FileSystem.CopyDirectory(dataPath, Path.Combine(newestBackupPath, "data"));
                }
                if (FileSystem.DirectoryExists(profilePath))
                {
                    string[] filePaths = FileSystem.GetFiles(profilePath).ToArray();
                    foreach (string filePath in filePaths)
                    {
                        if (Path.GetExtension(filePath) == ".ADM" || Path.GetExtension(filePath) == ".RPT" || Path.GetExtension(filePath) == ".log" || Path.GetExtension(filePath) == ".mdmp")
                        {
                            FileSystem.MoveFile(filePath, Path.Combine(newestBackupPath, "logs", Path.GetFileName(filePath)));
                        }
                    }
                }
                Manager.WriteToConsole($"Server backup and moving of the logs done");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public static void DeleteOldBackups(string backupPath, int maxKeepTime)
        {
            try
            {
                if (FileSystem.DirectoryExists(backupPath))
                {
                    DateTime dateTreshold = DateTime.Now.AddDays(-maxKeepTime);
                    List<string> backupFolders = FileSystem.GetDirectories(backupPath).ToList();
                    foreach (string folder in backupFolders)
                    {
                        DateTime folderDate;
                        bool isValidDate = DateTime.TryParseExact(Path.GetFileName(folder), "yyyy-MM-dd HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out folderDate);
                        if (isValidDate && folderDate < dateTreshold)
                        {
                            FileSystem.DeleteDirectory(folder, DeleteDirectoryOption.DeleteAllContents);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }
    }
}
