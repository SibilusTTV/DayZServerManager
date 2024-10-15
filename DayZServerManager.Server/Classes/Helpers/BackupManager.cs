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
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
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
                string dataPath = Path.Combine(Manager.MPMISSIONS_PATH, missionName, Manager.PERSISTANCE_FOLDER_NAME);
                string profilePath = Path.Combine(Manager.SERVER_PATH, profileName);
                if (Directory.Exists(dataPath))
                {
                    FileSystem.CopyDirectory(dataPath, Path.Combine(newestBackupPath, Manager.BACKUP_DATA_FOLDER_NAME), true);
                }
                if (Directory.Exists(profilePath))
                {
                    if (!Directory.Exists(Path.Combine(newestBackupPath, Manager.BACKUP_LOGS_FOLDER_NAME)))
                    {
                        Directory.CreateDirectory(Path.Combine(newestBackupPath, Manager.BACKUP_LOGS_FOLDER_NAME));
                    }
                    MoveLogsInDirectory(profilePath, newestBackupPath);
                }
                Manager.WriteToConsole($"Server backup and moving of the logs done");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public static void MoveLogsInDirectory(string folderPath, string newestBackupPath)
        {
            string[] filePaths = Directory.GetFiles(folderPath).ToArray();
            foreach (string filePath in filePaths)
            {
                if (Manager.LogsNames.Contains(Path.GetExtension(filePath)))
                {
                    File.Move(filePath, Path.Combine(newestBackupPath, Manager.BACKUP_LOGS_FOLDER_NAME, Path.GetFileName(filePath)), true);
                }
            }

            string[] folderPaths = Directory.GetDirectories(folderPath).ToArray();
            foreach (string nextFolderPath in folderPaths)
            {
                MoveLogsInDirectory(nextFolderPath, newestBackupPath);
            }
        }

        public static void DeleteOldBackups(string backupPath, int maxKeepTime)
        {
            try
            {
                if (Directory.Exists(backupPath))
                {
                    DateTime dateTreshold = DateTime.Now.AddDays(-maxKeepTime);
                    List<string> backupFolders = Directory.GetDirectories(backupPath).ToList();
                    foreach (string folder in backupFolders)
                    {
                        DateTime folderDate;
                        bool isValidDate = DateTime.TryParseExact(Path.GetFileName(folder), "yyyy-MM-dd HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out folderDate);
                        if (isValidDate && folderDate < dateTreshold)
                        {
                            Directory.Delete(folder, true);
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
