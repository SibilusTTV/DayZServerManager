using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;

namespace DayZServerManager.Server.Classes.Handlers.SteamCMDHandler
{
    public static class SteamCMDManager
    {
        public static Process? steamCMDProcess;

        public static void UpdateServer(ManagerProps props)
        {
            if (Manager.managerConfig != null && Manager.props != null)
            {
                props.managerStatus = Manager.STATUS_UPDATING_SERVER;
                Manager.WriteToConsole(Manager.STATUS_UPDATING_SERVER);

                try
                {
                    if (!Directory.Exists(Manager.STEAM_CMD_PATH))
                    {
                        Directory.CreateDirectory(Manager.STEAM_CMD_PATH);
                    }
                }
                catch (Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
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
                    Manager.WriteToConsole(ex.ToString());
                }

                try
                {
                    string serverUpdateArguments = $"\"+force_install_dir {Path.Combine("..", Manager.SERVER_DEPLOY)}\" \"+login {Manager.managerConfig.steamUsername}\" \"+app_update {Manager.DAYZ_SERVER_BRANCH}\" -validate +quit";
                    Manager.WriteToConsole("Updating the DayZ Server");
                    StartSteamCMD(props, serverUpdateArguments);
                    if (props.steamCMDStatus == Manager.STATUS_NOT_RUNNING)
                    {
                        CheckForUpdatedServer();
                    }
                }
                catch (Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
                    props.managerStatus = Manager.STATUS_ERROR;
                }

                props.managerStatus = Manager.STATUS_SERVER_UPDATED;
                Manager.WriteToConsole(Manager.STATUS_SERVER_UPDATED);
            }
        }

        public static bool UpdateMods(ManagerProps props, List<Mod> mods, out List<long> updatedModsIDs)
        {
            updatedModsIDs = new List<long>();
            bool updatedMods = false;
            if (Manager.managerConfig != null)
            {
                props.managerStatus = Manager.STATUS_UPDATING_MODS;
                Manager.WriteToConsole(Manager.STATUS_UPDATING_MODS);

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

                        StartSteamCMD(props, arguments);

                        Manager.WriteToConsole($"All mods were downloaded");

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
                                    Manager.WriteToConsole($"{mod.name} was updated");
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
                                Manager.WriteToConsole($"{mod.name} was updated");
                            }
                        }
                    }
                    return updatedMods;
                }
                catch (Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
                }

                props.managerStatus = Manager.STATUS_MODS_UPDATED;
                Manager.WriteToConsole(Manager.STATUS_MODS_UPDATED);
            }
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
                        return "Steam guard written";
                    }
                }
                else
                {
                    if (Manager.props != null) Manager.props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
                    return "No SteamCMD Process";
                }

                return "Server was downloaded";
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
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
                    Manager.WriteToConsole("DayZ Server updated");
                    return true;
                }
                else
                {
                    Manager.WriteToConsole("Server was already up-to-date");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
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
                Manager.WriteToConsole(ex.ToString());
            }
        }

        private static void StartSteamCMD(ManagerProps props, string serverUpdateArguments)
        {
            try
            {
                steamCMDProcess = new Process();
                steamCMDProcess.StartInfo.UseShellExecute = false;
                steamCMDProcess.StartInfo.Arguments = serverUpdateArguments;
                steamCMDProcess.StartInfo.RedirectStandardError = true;
                steamCMDProcess.StartInfo.RedirectStandardInput = true;
                steamCMDProcess.StartInfo.RedirectStandardOutput = true;
                Manager.WriteToConsole(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE) + " " + serverUpdateArguments);
                steamCMDProcess.StartInfo.FileName = Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE);
                steamCMDProcess.Start();

                props.steamCMDStatus = Manager.STATUS_RUNNING;

                int outputTime = 0;
                Task task = ConsumeOutput(steamCMDProcess.StandardOutput, s =>
                {
                    if (s != null)
                    {
                        Manager.WriteToConsole(s);
                        if (s.ToLower().Contains(Manager.STATUS_CLIENT_CONFIG.ToLower()))
                        {
                            props.steamCMDStatus = Manager.STATUS_CLIENT_CONFIG;
                            outputTime = -1;
                        }
                        else if (s.ToLower().Contains(Manager.STATUS_STEAM_GUARD.ToLower()))
                        {
                            props.steamCMDStatus = Manager.STATUS_STEAM_GUARD;
                        }
                        else if (s.ToLower().Contains(Manager.STATUS_CACHED_CREDENTIALS.ToLower()))
                        {
                            props.steamCMDStatus = Manager.STATUS_CACHED_CREDENTIALS;
                        }
                        else if (outputTime != -1)
                        {
                            outputTime = 0;
                        }
                    }
                });

                while (!task.IsCompleted)
                {
                    Thread.Sleep(1000);
                    if (outputTime > 30)
                    {
                        if (props.steamCMDStatus == Manager.STATUS_CACHED_CREDENTIALS)
                        {
                            Manager.WriteToConsole(Manager.STATUS_STEAM_GUARD);
                            props.steamCMDStatus = Manager.STATUS_STEAM_GUARD;
                            outputTime = 0;
                        }
                        else
                        {
                            Manager.WriteToConsole(Manager.STATUS_CACHED_CREDENTIALS);
                            steamCMDProcess.StandardInput.WriteLine(Manager.managerConfig?.steamPassword);
                            props.steamCMDStatus = Manager.STATUS_CACHED_CREDENTIALS;
                            outputTime = 0;
                        }
                    }
                    else if (outputTime != -1)
                    {
                        outputTime++;
                    }
                }

                steamCMDProcess.WaitForExit();

                props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                props.dayzServerStatus = Manager.STATUS_ERROR;
            }
        }

        private static async Task ConsumeOutput(TextReader reader, Action<string> callback)
        {
            char[] buffer = new char[256];
            int cch;

            while ((cch = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                callback(new string(buffer, 0, cch));
            }
        }

        public static void KillSteamCMD()
        {
            try
            {
                if (steamCMDProcess != null && Manager.props != null)
                {
                    steamCMDProcess.Kill();
                    steamCMDProcess = null;
                    Manager.props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
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
                Manager.WriteToConsole(ex.ToString());
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
                Manager.WriteToConsole(ex.ToString());
                return false;
            }
        }
    }
}
