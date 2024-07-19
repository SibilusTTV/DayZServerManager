using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using LibGit2Sharp;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes
{
    internal class Server
    {
        // Server Variables
        private ManagerConfig config;
        int dayZServerBranch;
        int dayZGameBranch;

        // Other Variables
        private bool updatedMods;
        private bool restartingForUpdates;
        private bool updatedServer;
        private List<long> updatedModsIDs;

        Process? serverProcess;
        Process? becProcess;
        Process? becUpdateProcess;

        public Server(ManagerConfig config)
        {
            this.config = config;
            dayZServerBranch = 223350;
            dayZGameBranch = 221100;
            serverProcess = null;
            becProcess = null;
            becUpdateProcess = null;
            updatedModsIDs = new List<long>();
        }

        public bool CheckServer()
        {
            if (serverProcess != null)
            {
                return !serverProcess.HasExited;
            }
            else
            {
                return false;
            }
        }

        public bool CheckBEC()
        {
            if (becProcess != null && !restartingForUpdates)
            {
                return !becProcess.HasExited;
            }
            else if (restartingForUpdates)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StartServer()
        {
            updatedModsIDs = new List<long>();
            updatedMods = false;
            restartingForUpdates = false;
            updatedServer = false;
            string clientModsToLoad = string.Empty;
            foreach (Mod clientMod in config.clientMods)
            {
                if (clientMod != null)
                {
                    clientModsToLoad += clientMod.name + ";";
                }
            }
            if (!string.IsNullOrEmpty(clientModsToLoad))
            {
                clientModsToLoad = $"\"-mod={clientModsToLoad.Remove(clientModsToLoad.Length - 1)}\"";
            }

            string serverModsToLoad = string.Empty;
            foreach (Mod serverMod in config.serverMods)
            {
                if (serverMod != null)
                {
                    serverModsToLoad += serverMod + ";";
                }
            }
            if (!string.IsNullOrEmpty(serverModsToLoad))
            {
                serverModsToLoad = $"\"-serverMod={serverModsToLoad.Remove(serverModsToLoad.Length - 1)}\"";
            }

            try
            {
                serverProcess = new Process();
                ProcessStartInfo procInf = new ProcessStartInfo();
                string startParameters = GetServerStartParameters(clientModsToLoad, serverModsToLoad);
                procInf.WorkingDirectory = config.serverPath;
                procInf.Arguments = startParameters;
                procInf.FileName = Path.Combine(config.serverPath, "DayZServer_x64.exe");
                procInf.RedirectStandardOutput = true;
                serverProcess.StartInfo = procInf;
                WriteToConsole($"Starting Server");
                serverProcess.Start();
                WriteToConsole($"Server starting at {Path.Combine(config.serverPath, "DayZServer_x64.exe")} with the parameters {startParameters}");
                string lines = serverProcess.StandardOutput.ReadToEnd();
                Console.WriteLine(lines);

            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        private string GetServerStartParameters(string clientModsToLoad, string serverModsToLoad)
        {
            string parameters = $"-instanceId={config.instanceId} -config={config.serverConfigName} -profiles={config.profileName} -port={config.port} {clientModsToLoad} {serverModsToLoad} -cpuCount={config.cpuCount}";

            if (config.noFilePatching)
            {
                parameters += " -noFilePatching";
            }
            if (config.doLogs)
            {
                parameters += " -doLogs";
            }
            if (config.adminLog)
            {
                parameters += " -adminLog";
            }
            if (config.freezeCheck)
            {
                parameters += " -freezeCheck";
            }
            if (config.netLog)
            {
                parameters += " -netLog";
            }
            if (config.limitFPS > 0)
            {
                parameters += $" -limitFPS={config.limitFPS}";
            }

            return parameters;
        }

        public void UpdateBEC()
        {
            try
            {
                if (!FileSystem.DirectoryExists(config.becPath))
                {
                    FileSystem.CreateDirectory(config.becPath);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                if (!FileSystem.FileExists(Path.Combine(config.becPath, "Bec.exe")))
                {
                    Repository.Clone("https://github.com/TheGamingChief/BattlEye-Extended-Controls.git", config.becPath);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                if (FileSystem.FileExists(Path.Combine(config.becPath, "Config", "Config.cfg")))
                {
                    string becConfig;

                    using (StreamReader reader = new StreamReader(Path.Combine(config.becPath, "Config", "Config.cfg")))
                    {
                        becConfig = reader.ReadToEnd();
                    }

                    becConfig = becConfig.Replace("C:\\Servers\\DayZ\\ServerName\\BattlEye", Path.Combine(Environment.CurrentDirectory, config.serverPath, config.profileName, "BattlEye"));
                    string defaultPort = becConfig.Substring(becConfig.IndexOf("Port = "), becConfig.IndexOf(System.Environment.NewLine, becConfig.IndexOf("Port = ") - 1) - becConfig.IndexOf("Port = "));
                    becConfig = becConfig.Replace(defaultPort, $"Port = {config.RConPort}");
                    string defaultTimeout = becConfig.Substring(becConfig.IndexOf("Timeout = "), becConfig.IndexOf(System.Environment.NewLine, becConfig.IndexOf("Timeout = ") - 1) - becConfig.IndexOf("Timeout = "));
                    becConfig = becConfig.Replace(defaultTimeout, "Timeout = 60");

                    using (StreamWriter writer = new StreamWriter(Path.Combine(config.becPath, "Config", "Config.cfg")))
                    {
                        writer.Write(becConfig);
                        writer.Close();
                    }

                    string defaultScheduler = becConfig.Substring(becConfig.IndexOf("Scheduler = "), becConfig.IndexOf(System.Environment.NewLine, becConfig.IndexOf("Scheduler = ") - 1) - becConfig.IndexOf("Scheduler = "));
                    becConfig = becConfig.Replace(defaultScheduler, "Scheduler = SchedulerUpdate.xml");

                    using (StreamWriter writer = new StreamWriter(Path.Combine(config.becPath, "Config", "ConfigUpdate.cfg")))
                    {
                        writer.Write(becConfig);
                        writer.Close();
                    }
                }

                if (!FileSystem.DirectoryExists(Path.Combine(config.serverPath, config.profileName, "BattlEye")))
                {
                    FileSystem.CreateDirectory(Path.Combine(config.serverPath, config.profileName, "BattlEye"));
                }

                if (!FileSystem.FileExists(Path.Combine(config.serverPath, config.profileName, "BattlEye", "BEServer_x64.cfg")))
                {
                    string beConfig = "RConPassword YourRCONPasswort";
                    beConfig += $"{Environment.NewLine}RConPort {config.RConPort}";

                    using (StreamWriter writer = new StreamWriter(Path.Combine(config.serverPath, config.profileName, "BattlEye", "BEServer_x64.cfg")))
                    {
                        writer.Write(beConfig);
                        writer.Close();
                    }
                }

                if (!FileSystem.FileExists(Path.Combine(config.serverPath, config.profileName, "BattlEye", "bans.txt")))
                {
                    string bans = "";

                    using (StreamWriter writer = new StreamWriter(Path.Combine(config.serverPath, config.profileName, "BattlEye", "bans.txt")))
                    {
                        writer.Write(bans);
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public void StartBEC()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.FileName = Path.Combine(config.becPath, "Bec.exe");
                startInfo.Arguments = $"-f Config.cfg --dsc";
                startInfo.WorkingDirectory = config.becPath;
                becProcess = new Process();
                becProcess.StartInfo = startInfo;
                WriteToConsole("Starting BEC");
                becProcess.Start();
                WriteToConsole("BEC started");
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public void KillServerProcesses()
        {
            try
            {
                if (serverProcess != null)
                {
                    serverProcess.Kill();
                }
                if (becProcess != null)
                {
                    becProcess.Kill();
                }
                if (becUpdateProcess != null)
                {
                    becUpdateProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public void UpdateServer()
        {
            try
            {
                if (!FileSystem.DirectoryExists(config.steamCMDPath))
                {
                    FileSystem.CreateDirectory(config.steamCMDPath);
                }
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                if (!FileSystem.FileExists(Path.Combine(config.steamCMDPath, "steamcmd.exe")))
                {
                    string zipName = "steamcmd.zip";
                    DownloadAndExctractSteamCMD(zipName);
                }
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                DateTime dateBeforeUpdate;
                if (FileSystem.FileExists(Path.Combine(config.serverPath, "DayZServer_x64.exe")))
                {
                    dateBeforeUpdate = File.GetLastWriteTimeUtc(Path.Combine(config.serverPath, "DayZServer_x64.exe"));
                }
                else
                {
                    dateBeforeUpdate = DateTime.MinValue;
                }

                string serverUpdateArguments = $"+force_install_dir {Path.Combine("..", config.serverPath)} +login {config.steamUsername} {config.steamPassword} +\"app_update {dayZServerBranch}\" +quit";
                WriteToConsole("Updating the DayZ Server");
                Process p = Process.Start(Path.Combine(config.steamCMDPath, "steamcmd.exe"), serverUpdateArguments);
                p.WaitForExit();


                DateTime dateAfterUpdate;
                if (FileSystem.FileExists(Path.Combine(config.serverPath, "DayZServer_x64.exe")))
                {
                    dateAfterUpdate = File.GetLastWriteTimeUtc(Path.Combine(config.serverPath, "DayZServer_x64.exe"));
                }
                else
                {
                    dateAfterUpdate = DateTime.Now;
                }
                
                if (dateBeforeUpdate < dateAfterUpdate)
                {
                    updatedServer = true;
                    WriteToConsole("DayZ Server updated");
                }
                else
                {
                    updatedServer = false;
                    WriteToConsole("Server was already up-to-date");
                }
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public void UpdateAndMoveMods(bool hasToUpdate, bool hasToMove)
        {
            List<Mod> mods = new List<Mod>();
            mods.AddRange(config.clientMods);
            mods.AddRange(config.serverMods);
            if (mods.Count > 0)
            {
                if (hasToUpdate)
                {
                    WriteToConsole("Updating Mods");
                    UpdateMods(mods);
                    WriteToConsole("Mods updated");
                }
                if (hasToMove)
                {
                    WriteToConsole("Moving Mods");
                    MoveMods(mods);
                    WriteToConsole("Mods moved");
                }

                if ((config.clientMods.FindAll(x => x.name.Contains("expansion") || x.name.Contains("Expansion")).Count > 0 && updatedModsIDs.Contains(config.clientMods.Find(x => x.name.Contains("expansion") || x.name.Contains("Expansion")).workshopID)) || updatedServer)
                {
                    updatedServer = false;
                    WriteToConsole($"Updating Mission folder");
                    MissionUpdater upd = new MissionUpdater(config);
                    upd.Update();
                    WriteToConsole($"Finished updating Mission folder");
                }
            }
        }

        public void UpdateMods(List<Mod> mods)
        {
            try
            {
                string modUpdateArguments = string.Empty;
                if (mods.Count > 0)
                {
                    foreach (Mod mod in mods)
                    {
                        modUpdateArguments += $" +workshop_download_item {dayZGameBranch} {mod.workshopID.ToString()}";
                    }
                    string arguments = $"+login {config.steamUsername} {config.steamPassword}{modUpdateArguments} +quit";
                    WriteToConsole($"{Path.Combine(config.steamCMDPath, "steamcmd.exe")} {arguments}");
                    Process p = Process.Start(Path.Combine(config.steamCMDPath, "steamcmd.exe"), arguments);
                    p.WaitForExit();
                    WriteToConsole($"All mods were downloaded");
                    foreach (Mod mod in mods)
                    {
                        if (CompareForChanges(Path.Combine(config.workshopPath, mod.workshopID.ToString()), Path.Combine(config.serverPath, mod.name)))
                        {
                            if (updatedModsIDs.Contains(mod.workshopID))
                            {
                                updatedMods = true;
                            }
                            else
                            {
                                updatedModsIDs.Add(mod.workshopID);
                                updatedMods = true;
                            }
                        }
                    }

                    foreach (long key in updatedModsIDs)
                    {
                        Mod? mod = SearchForMod(key, mods);
                        if (mod != null)
                        {
                            WriteToConsole($"{mod.name} was updated");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public void MoveMods(List<Mod> mods)
        {
            try
            {
                foreach (long key in updatedModsIDs)
                {
                    Mod? mod = SearchForMod(key, mods);
                    if (mod != null)
                    {
                        string steamModPath = Path.Combine(config.workshopPath, mod.workshopID.ToString());
                        string serverModPath = Path.Combine(config.serverPath, mod.name);

                        WriteToConsole($"Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                        if (FileSystem.DirectoryExists(steamModPath))
                        {
                            FileSystem.CopyDirectory(steamModPath, serverModPath, true);

                            string serverKeysPath = GetKeysFolder(config.serverPath);
                            string modKeysPath = GetKeysFolder(serverModPath);

                            if (modKeysPath != string.Empty && serverKeysPath != string.Empty)
                            {
                                FileSystem.CopyDirectory(modKeysPath, serverKeysPath, true);
                            }
                            WriteToConsole($"Mod was moved to {mod.name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        public bool CheckForUpdatedMods()
        {
            if (config.restartOnUpdate && !restartingForUpdates && ((updatedMods && updatedModsIDs != null && updatedModsIDs.Count > 0) || updatedServer) && !(becUpdateProcess != null && becUpdateProcess.HasExited))
            {
                updatedMods = false;
                updatedServer = false;
                try
                {
                    SchedulerFile? schedulerFile = SchedulerFileSerializer.DeserializeSchedulerFile(Path.Combine(config.becPath, "Config", "configUpdate.xml"));

                    if (schedulerFile == null)
                    {
                        schedulerFile = new SchedulerFile();
                    }

                    if (RestartUpdater.UpdateOnUpdateRestartScript(config.restartInterval, DateTime.Now, schedulerFile))
                    {
                        restartingForUpdates = true;
                        if (becProcess != null && !becProcess.HasExited)
                        {
                            becProcess.Kill();
                        }

                        SchedulerFileSerializer.SerializeSchedulerFile(Path.Combine(config.becPath, "Config", "SchedulerUpdate.xml"), schedulerFile);

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.FileName = Path.Combine(config.becPath, "Bec.exe");
                        startInfo.Arguments = $"-f ConfigUpdate.cfg --dsc";
                        startInfo.WorkingDirectory = config.becPath;
                        becUpdateProcess = new Process();
                        becUpdateProcess.StartInfo = startInfo;
                        WriteToConsole("Starting BEC for mod updates");
                        becUpdateProcess.Start();
                        WriteToConsole("BEC for mod updates started");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    WriteToConsole(ex.ToString());
                    return false;
                }
            }
            else
            {
                updatedMods = false;
                updatedServer = false;
            }
            return false;
        }

        public void BackupServerData()
        {
            try
            {
                if (!FileSystem.DirectoryExists(config.backupPath))
                {
                    FileSystem.CreateDirectory(config.backupPath);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                WriteToConsole($"Backing up the server data and moving all the logs!");
                string newestBackupPath = Path.Combine(config.backupPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                string dataPath = Path.Combine(config.serverPath, "mpmissions", config.missionName, "storage_1");
                string profilePath = Path.Combine(config.serverPath, config.profileName);
                if (FileSystem.DirectoryExists(dataPath))
                {
                    FileSystem.CopyDirectory(dataPath, Path.Combine(newestBackupPath, "data"));
                }
                if (FileSystem.DirectoryExists(profilePath))
                {
                    string[] fileNames = FileSystem.GetFiles(profilePath).ToArray();
                    foreach (string fileName in fileNames)
                    {
                        if (fileName.EndsWith(".ADM") || fileName.EndsWith(".RPT") || fileName.EndsWith(".log"))
                        {
                            FileSystem.MoveFile(fileName, Path.Combine(newestBackupPath, "logs", fileName.Substring(fileName.LastIndexOf("\\") + 1)));
                        }
                    }
                }
                WriteToConsole($"Server backup and moving of the logs done");
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        private Mod? SearchForMod(long key, List<Mod> mods)
        {
            foreach (Mod mod in mods)
            {
                if (mod.workshopID == key)
                {
                    return mod;
                }
            }

            return null;
        }

        private void DownloadAndExctractSteamCMD(string zipName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] content = response.Content.ReadAsByteArrayAsync().Result;
                        File.WriteAllBytes(Path.Combine(config.steamCMDPath, zipName), content);
                        ZipFile.ExtractToDirectory(Path.Combine(config.steamCMDPath, zipName), config.steamCMDPath);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        private string GetKeysFolder(string folderPath)
        {
            try
            {
                if (FileSystem.DirectoryExists(Path.Combine(folderPath, "Keys")))
                {
                    return Path.Combine(folderPath, "Keys");
                }
                else if (FileSystem.DirectoryExists(Path.Combine(folderPath, "Key")))
                {
                    return Path.Combine(folderPath, "Key");
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                return string.Empty;
            }
        }

        private void WriteToConsole(string message)
        {
            System.Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
        }

        private bool CompareForChanges(string steamModPath, string serverModPath)
        {
            List<string> steamModFilePaths = FileSystem.GetFiles(steamModPath).ToList<string>();
            foreach (string filePath in steamModFilePaths)
            {
                if (CheckFile(filePath, serverModPath))
                {
                    return true;
                }
            }

            List<string> steamModDirectoryPaths = FileSystem.GetDirectories(steamModPath).ToList<string>();
            foreach (string directoryPath in steamModDirectoryPaths)
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
                string serverDirectoryPath = Path.Combine(serverModPath, steamDirectoryPath.Substring(steamDirectoryPath.LastIndexOf("\\") + 1));
                if (FileSystem.DirectoryExists(serverModPath) && FileSystem.DirectoryExists(serverDirectoryPath))
                {
                    List<string> steamModFilePaths = FileSystem.GetFiles(steamDirectoryPath).ToList<string>();
                    foreach (string filePath in steamModFilePaths)
                    {
                        if (CheckFile(filePath, serverDirectoryPath))
                        {
                            return true;
                        }
                    }

                    List<string> steamModDirectoryPaths = FileSystem.GetDirectories(steamDirectoryPath).ToList<string>();
                    foreach (string directoryPath in steamModDirectoryPaths)
                    {
                        if (CheckDirectories(directoryPath, serverDirectoryPath))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else if (FileSystem.DirectoryExists(serverModPath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                WriteToConsole(ex.ToString());
                return false;
            }
        }

        private bool CheckFile(string steamFilePath, string serverModPath)
        {
            try
            {
                string serverFilePath = Path.Combine(serverModPath, steamFilePath.Substring(steamFilePath.LastIndexOf("\\") + 1));
                if (FileSystem.FileExists(steamFilePath) && FileSystem.FileExists(serverFilePath))
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
                else if (FileSystem.FileExists(steamFilePath))
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
                WriteToConsole(ex.ToString());
                return false;
            }
        }

        #region SchedulerFile
        #endregion SchedulerFile
    }
}