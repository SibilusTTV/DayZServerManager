
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;

namespace DayZServerManager.Server.Classes.Handlers.SteamCMDHandler
{
    public static class SteamCMDManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static Process? steamCMDProcess;

        public static bool UpdateServer()
        {
            Manager.props.managerStatus = Manager.STATUS_UPDATING_SERVER;
            Logger.Info(Manager.STATUS_UPDATING_SERVER);

            try
            {
                if (!Directory.Exists(Manager.STEAM_CMD_PATH))
                {
                    Directory.CreateDirectory(Manager.STEAM_CMD_PATH);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when creating steamCmd path");
            }

            try
            {
                if (!File.Exists(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE)))
                {
                    DownloadAndExctractSteamCMD(Manager.STEAM_CMD_ZIP_NAME);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when downloading and extracting steamCmd");
            }

            try
            {
                string serverUpdateArguments = $"\"+force_install_dir {Path.Combine("..", Manager.SERVER_DEPLOY)}\" \"+login {Manager.managerConfig.steamUsername}\" \"+app_update {Manager.DAYZ_SERVER_BRANCH}\" -validate +quit";
                Logger.Info("Updating the DayZ Server");
                StartSteamCMD(serverUpdateArguments);
                if (Manager.props.steamCMDStatus == Manager.STATUS_NOT_RUNNING)
                {
                    return CheckForUpdatedServer();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating server");
                Manager.props.managerStatus = Manager.STATUS_ERROR;
                return false;
            }

            Manager.props.managerStatus = Manager.STATUS_SERVER_UPDATED;
            Logger.Info(Manager.STATUS_SERVER_UPDATED);
            return false;
        }

        public static bool UpdateMods(List<Mod> mods, out List<long> updatedModsIDs)
        {
            updatedModsIDs = new List<long>();
            bool updatedMods = false;

            Manager.props.managerStatus = Manager.STATUS_UPDATING_MODS;
            Logger.Info(Manager.STATUS_UPDATING_MODS);

            try
            {
                string modUpdateArguments = string.Empty;
                if (mods.Count > 0)
                {
                    foreach (Mod mod in mods)
                    {
                        modUpdateArguments += $" +workshop_download_item {Manager.DAYZ_GAME_BRANCH} {mod.workshopID.ToString()}";
                    }
                    string arguments = $"\"+force_install_dir {Path.Combine("..", Manager.MODS_PATH)}\" \"+login {Manager.managerConfig.steamUsername}\" {modUpdateArguments} +quit";

                    StartSteamCMD(arguments);

                    Logger.Info($"All mods were downloaded");

                    foreach (Mod mod in mods)
                    {
                        if (CompareForChanges(Path.Combine(Manager.MODS_PATH, Manager.WORKSHOP_PATH, mod.workshopID.ToString()), Path.Combine(Manager.SERVER_PATH, mod.name)))
                        {
                            if (updatedModsIDs.Contains(mod.workshopID))
                            {
                                updatedMods = true;
                            }
                            else
                            {
                                Logger.Info($"{mod.name} was updated");
                                updatedModsIDs.Add(mod.workshopID);
                                updatedMods = true;
                            }
                        }
                    }

                    foreach (long key in updatedModsIDs)
                    {
                        Mod? mod = mods.Find(x => x.workshopID == key);
                        if (mod != null)
                        {
                            Logger.Info($"{mod.name} was updated");
                        }
                    }
                }
                return updatedMods;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating mods");
            }

            Manager.props.managerStatus = Manager.STATUS_MODS_UPDATED;
            Logger.Info(Manager.STATUS_MODS_UPDATED);

            return updatedMods;
        }

        public static string WriteSteamGuard(string code)
        {
            try
            {
                if (steamCMDProcess != null)
                {
                    if (!steamCMDProcess.HasExited)
                    {
                        steamCMDProcess.StandardInput.WriteLine(code);
                        Logger.Info("Steam guard written");
                        return "Steam guard written";
                    }
                }
                else
                {
                    Manager.props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
                    return "No SteamCMD Process";
                }

                return "Server was downloaded";
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when writing SteamGuard");
                return "Error";
            }
        }

        private static bool CheckForUpdatedServer()
        {
            try
            {
                DateTime dateBeforeUpdate;
                if (File.Exists(Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE)))
                {
                    dateBeforeUpdate = File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE));
                }
                else
                {
                    dateBeforeUpdate = DateTime.MinValue;
                }

                DateTime dateAfterUpdate;
                if (File.Exists(Path.Combine(Manager.SERVER_DEPLOY, Manager.SERVER_EXECUTABLE)))
                {
                    dateAfterUpdate = File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_DEPLOY, Manager.SERVER_EXECUTABLE));
                }
                else
                {
                    dateAfterUpdate = DateTime.MaxValue;
                }

                if (dateBeforeUpdate < dateAfterUpdate)
                {
                    Logger.Info("DayZ Server updated");
                    return true;
                }
                else
                {
                    Logger.Info("Server was already up-to-date");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when checking updated status of the server");
                return false;
            }
        }

        private static bool CompareForChanges(string steamModPath, string serverModPath)
        {
            List<string> steamModFilePaths = Directory.GetFiles(steamModPath).ToList();
            foreach (string filePath in steamModFilePaths)
            {
                if (CheckFile(filePath, serverModPath))
                {
                    return true;
                }
            }

            List<string> steamModDirectoryPaths = Directory.GetDirectories(steamModPath).ToList();
            foreach (string directoryPath in steamModDirectoryPaths)
            {
                if (CheckDirectories(directoryPath, serverModPath))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CheckSteamCMD()
        {
            return steamCMDProcess != null && !steamCMDProcess.HasExited;
        }

        private static void DownloadAndExctractSteamCMD(string zipName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    response = client.GetAsync(Manager.STEAMCMD_DOWNLOAD_URL + Manager.STEAM_CMD_ZIP_NAME).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] content = response.Content.ReadAsByteArrayAsync().Result;
                        File.WriteAllBytes(Path.Combine(Manager.STEAM_CMD_PATH, zipName), content);
                        if (OperatingSystem.IsWindows())
                        {
                            ZipFile.ExtractToDirectory(Path.Combine(Manager.STEAM_CMD_PATH, zipName), Manager.STEAM_CMD_PATH);
                        }
                        else
                        {
                            using (FileStream compressedFileStream = File.Open(Path.Combine(Manager.STEAM_CMD_PATH, zipName), FileMode.Open))
                            {
                                using (FileStream outputFileStream = File.Create(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_TAR_FILE_NAME)))
                                {
                                    using (var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                                    {
                                        decompressor.CopyTo(outputFileStream);
                                    }
                                }
                            }
                            if (File.Exists(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_TAR_FILE_NAME)))
                            {
                                TarFile.ExtractToDirectory(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_TAR_FILE_NAME), Manager.STEAM_CMD_PATH, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when downloading and extracting steamCmd");
            }
        }

        private static void StartSteamCMD(string serverUpdateArguments)
        {
            try
            {
                steamCMDProcess = new Process();
                steamCMDProcess.StartInfo.UseShellExecute = false;
                steamCMDProcess.StartInfo.Arguments = serverUpdateArguments;
                steamCMDProcess.StartInfo.RedirectStandardError = true;
                steamCMDProcess.StartInfo.RedirectStandardInput = true;
                steamCMDProcess.StartInfo.RedirectStandardOutput = true;
                Logger.Info(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE) + " " + serverUpdateArguments);
                steamCMDProcess.StartInfo.FileName = Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE);

                if (OperatingSystem.IsWindows())
                {
                    if (!Directory.Exists(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_LOGS_FOLDER)))
                    {
                        Directory.CreateDirectory(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_LOGS_FOLDER));
                    }
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_LINUX32_Folder)))
                    {
                        Directory.CreateDirectory(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_LINUX32_Folder));
                    }

                    if (!Directory.Exists(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_LINUX32_Folder, Manager.STEAMCMD_LOGS_FOLDER)))
                    {
                        Directory.CreateDirectory(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAMCMD_LINUX32_Folder, Manager.STEAMCMD_LOGS_FOLDER));
                    }
                }

                if (!File.Exists(Manager.STEAMCMD_CONSOLE_LOG_PATH))
                {
                    using (var fs = File.Create(Manager.STEAMCMD_CONSOLE_LOG_PATH))
                    {

                    }
                }

                steamCMDProcess.Start();

                Manager.props.steamCMDStatus = Manager.STATUS_RUNNING;

                using (var fs = new FileStream(Manager.STEAMCMD_CONSOLE_LOG_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.Default))
                    {
                        bool hasExited = false;
                        while (!steamCMDProcess.HasExited && !hasExited)
                        {
                            string? standardOutput = sr.ReadToEnd();

                            if (!string.IsNullOrEmpty(standardOutput))
                            {
                                Logger.Info(standardOutput);
                                if (standardOutput.Contains("password:"))
                                {
                                    Logger.Info(Manager.STATUS_CACHED_CREDENTIALS);
                                    steamCMDProcess.StandardInput.WriteLine(Manager.managerConfig.steamPassword);
                                    Manager.props.steamCMDStatus = Manager.STATUS_CACHED_CREDENTIALS;
                                }
                                else if (standardOutput.Contains("Steam Guard code:"))
                                {
                                    Logger.Info(Manager.STATUS_STEAM_GUARD);
                                    Manager.props.steamCMDStatus = Manager.STATUS_STEAM_GUARD;
                                }
                                else if (standardOutput.Contains("client config"))
                                {
                                    Manager.props.steamCMDStatus = Manager.STATUS_CLIENT_CONFIG;
                                }
                                else if (standardOutput.Contains("Unloading Steam API"))
                                {
                                    steamCMDProcess.WaitForExit();
                                    hasExited = true;
                                }
                            }
                        }

                        Logger.Info(sr.ReadToEnd());

                    }
                }

                if (steamCMDProcess != null)
                {
                    steamCMDProcess.Kill();
                    steamCMDProcess = null;
                }

                using (StreamWriter writer = new StreamWriter(Manager.STEAMCMD_CONSOLE_LOG_PATH, false))
                {
                    writer.Write(string.Empty);
                }

                Manager.props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when starting steam");
                Manager.props.dayzServerStatus = Manager.STATUS_ERROR;
            }
        }

        public static void KillSteamCMD()
        {
            try
            {
                if (steamCMDProcess != null)
                {
                    steamCMDProcess.Dispose();
                    steamCMDProcess = null;
                    Manager.props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when killing steamCmd");
                steamCMDProcess = null;
            }
        }

        private static bool CheckDirectories(string steamDirectoryPath, string serverModPath)
        {
            try
            {
                string serverDirectoryPath = Path.Combine(serverModPath, Path.GetFileName(steamDirectoryPath));
                if (Directory.Exists(serverModPath) && Directory.Exists(serverDirectoryPath))
                {
                    List<string> steamModFilePaths = Directory.GetFiles(steamDirectoryPath).ToList();
                    foreach (string filePath in steamModFilePaths)
                    {
                        if (CheckFile(filePath, serverDirectoryPath))
                        {
                            return true;
                        }
                    }

                    List<string> steamModDirectoryPaths = Directory.GetDirectories(steamDirectoryPath).ToList();
                    foreach (string directoryPath in steamModDirectoryPaths)
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
                Logger.Error(ex, "Error when checking directories");
                return false;
            }
        }

        private static bool CheckFile(string steamFilePath, string serverModPath)
        {
            try
            {
                string serverFilePath = Path.Combine(serverModPath, Path.GetFileName(steamFilePath));
                if (File.Exists(steamFilePath) && File.Exists(serverFilePath))
                {
                    DateTime steamModChangingDate = File.GetLastWriteTimeUtc(steamFilePath);
                    DateTime serverModChangingDate = File.GetLastWriteTimeUtc(serverFilePath);
                    if (steamModChangingDate > serverModChangingDate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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
                Logger.Error(ex, "Error when checking file");
                return false;
            }
        }
    }
}
