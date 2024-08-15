using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using LibGit2Sharp;
using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes
{
    internal class Server
    {
        // Server Variables
        private ManagerConfig config;
        int dayZServerBranch;
        int dayZGameBranch;
        private char folderSeparator;

        // Other Variables
        private bool updatedMods;
        private bool restartingForUpdates;
        private bool updatedServer;
        private List<long> updatedModsIDs;

        Process? serverProcess;
        Process? becProcess;
        Process? becUpdateProcess;
        Process? steamCMDProcess;

        public Server(ManagerConfig config)
        {
            this.config = config;
            dayZServerBranch = 223350;
            dayZGameBranch = 221100;
            serverProcess = null;
            becProcess = null;
            becUpdateProcess = null;
            updatedModsIDs = new List<long>();
            if (OperatingSystem.IsWindows())
            {
                folderSeparator = '\\';
            }
            else
            {
                folderSeparator = '/';
            }
        }

        public bool CheckServer()
        {
            try
            {
                if (serverProcess != null && !serverProcess.HasExited)
                {
                    return true;
                }
                else
                {
                    serverProcess = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                serverProcess = null;
                return false;
            }
        }

        public bool CheckBEC()
        {
            try
            {
                if (becProcess != null && !restartingForUpdates)
                {
                    if (becProcess.HasExited)
                    {
                        becProcess = null;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
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
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                becProcess = null;
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
                procInf.WorkingDirectory = Manager.SERVER_PATH;
                procInf.Arguments = startParameters;
                if (OperatingSystem.IsWindows())
                {
                    procInf.FileName = Path.Combine(Manager.SERVER_PATH, "DayZServer_x64.exe");
                }
                else
                {
                    procInf.FileName = Path.Combine(Manager.SERVER_PATH, "DayZServer");
                }
                serverProcess.StartInfo = procInf;
                WriteToConsole($"Starting Server");
                serverProcess.Start();
                if (OperatingSystem.IsWindows())
                {
                    WriteToConsole($"Server starting at {Path.Combine(Manager.SERVER_PATH, "DayZServer_x64.exe")} with the parameters {startParameters}");
                }
                else
                {
                    WriteToConsole($"Server starting at {Path.Combine(Manager.SERVER_PATH, "DayZServer")} with the parameters {startParameters}");
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        private string GetServerStartParameters(string clientModsToLoad, string serverModsToLoad)
        {
            string parameters = $"-instanceId={config.instanceId} \"-config={config.serverConfigName}\" \"-profiles={config.profileName}\" -port={config.port} {clientModsToLoad} {serverModsToLoad}-cpuCount={config.cpuCount}";

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
                if (!FileSystem.DirectoryExists(Manager.BEC_PATH))
                {
                    FileSystem.CreateDirectory(Manager.BEC_PATH);
                }
                if (!FileSystem.FileExists(Path.Combine(Manager.BEC_PATH, "Bec.exe")))
                {
                    Repository.Clone("https://github.com/TheGamingChief/BattlEye-Extended-Controls.git", Manager.BEC_PATH);
                }
                //else if ((!FileSystem.FileExists(Path.Combine(Manager.BEC_PATH, "Bec"))))
                //{
                //    Repository.Clone("https://github.com/TheGamingChief/BattlEye-Extended-Controls.git", Manager.BEC_PATH);
                //}
                else
                {
                    Repository rep = new Repository(Manager.BEC_PATH);
                    PullOptions pullOptions = new PullOptions();
                    pullOptions.FetchOptions = new FetchOptions();
                    Commands.Pull(rep, new Signature("username", "email", new DateTimeOffset(DateTime.Now)), pullOptions);
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                if (FileSystem.FileExists(Path.Combine(Manager.BEC_PATH, "Config", "Config.cfg")))
                {
                    string becConfig;

                    using (StreamReader reader = new StreamReader(Path.Combine(Manager.BEC_PATH, "Config", "Config.cfg")))
                    {
                        becConfig = reader.ReadToEnd();
                    }

                    becConfig = becConfig.Replace("C:\\Servers\\DayZ\\ServerName\\BattlEye", Path.Combine(Environment.CurrentDirectory, Manager.SERVER_PATH, config.profileName, "BattlEye"));

                    string portPattern = "Port\\s?=\\s?([0-9]+)";
                    Regex portReg = new Regex(portPattern);
                    Match portMatch = portReg.Match(becConfig);
                    if (portMatch.Success)
                    {
                        becConfig = becConfig.Replace(portMatch.Groups[1].Value, $"{config.RConPort}");
                    }

                    string timeoutPattern = "Timeout\\s?=\\s?([0-9]+)";
                    Regex timeoutReg = new Regex(timeoutPattern);
                    Match timeoutMatch = timeoutReg.Match(becConfig);
                    if (timeoutMatch.Success)
                    {
                        becConfig = becConfig.Replace(timeoutMatch.Groups[1].Value, "60");
                    }

                    using (StreamWriter writer = new StreamWriter(Path.Combine(Manager.BEC_PATH, "Config", "Config.cfg")))
                    {
                        writer.Write(becConfig);
                        writer.Close();
                    }

                    string schedulerPattern = "Scheduler\\s?=\\s?([^\\n\\r\\0\\t]+)";
                    Regex schedulerReg = new Regex(schedulerPattern);
                    Match schedulerMatch = schedulerReg.Match(becConfig);
                    if (schedulerMatch.Success)
                    {
                        becConfig = becConfig.Replace(schedulerMatch.Groups[1].Value, "SchedulerUpdate.xml");
                    }

                    using (StreamWriter writer = new StreamWriter(Path.Combine(Manager.BEC_PATH, "Config", "ConfigUpdate.cfg")))
                    {
                        writer.Write(becConfig);
                        writer.Close();
                    }
                }

                if (!FileSystem.DirectoryExists(Path.Combine(Manager.SERVER_PATH, config.profileName, "BattlEye")))
                {
                    FileSystem.CreateDirectory(Path.Combine(Manager.SERVER_PATH, config.profileName, "BattlEye"));
                }

                if (!FileSystem.FileExists(Path.Combine(Manager.SERVER_PATH, config.profileName, "BattlEye", "BEServer_x64.cfg")))
                {
                    string beConfig = "RConPassword YourRCONPasswort";
                    beConfig += $"{Environment.NewLine}RConPort {config.RConPort}";

                    using (StreamWriter writer = new StreamWriter(Path.Combine(Manager.SERVER_PATH, config.profileName, "BattlEye", "BEServer_x64.cfg")))
                    {
                        writer.Write(beConfig);
                        writer.Close();
                    }
                }

                if (!FileSystem.FileExists(Path.Combine(Manager.SERVER_PATH, config.profileName, "BattlEye", "bans.txt")))
                {
                    string bans = "";

                    using (StreamWriter writer = new StreamWriter(Path.Combine(Manager.SERVER_PATH, config.profileName, "BattlEye", "bans.txt")))
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
                if (OperatingSystem.IsWindows())
                {
                    startInfo.FileName = Path.Combine(Manager.BEC_PATH, "Bec.exe");
                }
                else
                {
                    startInfo.FileName = Path.Combine(Manager.BEC_PATH, "Bec");
                }
                startInfo.Arguments = $"-f Config.cfg --dsc";
                startInfo.WorkingDirectory = Manager.BEC_PATH;
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
                    serverProcess = null;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                serverProcess = null;
            }

            try
            {
                if (becProcess != null)
                {
                    becProcess.Kill();
                    becProcess = null;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                becProcess = null;
            }

            try
            {
                if (becUpdateProcess != null)
                {
                    becUpdateProcess.Kill();
                    becUpdateProcess = null;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                becUpdateProcess = null;
            }

            try
            {
                if (steamCMDProcess != null)
                {
                    steamCMDProcess.Kill();
                    steamCMDProcess = null;
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
                steamCMDProcess = null;
            }
        }

        public void UpdateServer(ManagerProps props)
        {
            try
            {
                if (!FileSystem.DirectoryExists(Manager.STEAM_CMD_PATH))
                {
                    FileSystem.CreateDirectory(Manager.STEAM_CMD_PATH);
                }
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                if (OperatingSystem.IsWindows() && !FileSystem.FileExists(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.exe")))
                {
                    string zipName = "steamcmd.zip";
                    DownloadAndExctractSteamCMD(zipName);
                }
                else if (!OperatingSystem.IsWindows() && !FileSystem.FileExists(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.sh")))
                {
                    string zipName = "steamcmd.tar.gz";
                    DownloadAndExctractSteamCMD(zipName);
                }
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            try
            {
                DateTime dateBeforeUpdate = GetDateBeforeUpdate();

                string serverUpdateArguments = $"+force_install_dir {Path.Combine("..", Manager.SERVER_PATH)} +\"login {config.steamUsername} {config.steamPassword}\" +\"app_update {dayZServerBranch}\" +quit";
                WriteToConsole("Updating the DayZ Server");
                StartSteamCMD(props, serverUpdateArguments);
                if (props._serverStatus == "Server downloaded")
                {
                    CheckForUpdatedServer(dateBeforeUpdate);
                }
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
                props._serverStatus = "Error";
            }
        }

        private void StartSteamCMD(ManagerProps props, string serverUpdateArguments)
        {
            try
            {
                steamCMDProcess = new Process();
                steamCMDProcess.StartInfo.UseShellExecute = false;
                steamCMDProcess.StartInfo.Arguments = serverUpdateArguments;
                steamCMDProcess.StartInfo.RedirectStandardError = true;
                steamCMDProcess.StartInfo.RedirectStandardInput = true;
                steamCMDProcess.StartInfo.RedirectStandardOutput = true;
                if (OperatingSystem.IsWindows())
                {
                    WriteToConsole(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.exe") + " " + serverUpdateArguments);
                    steamCMDProcess.StartInfo.FileName = Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.exe");
                    steamCMDProcess.Start();
                }
                else
                {
                    WriteToConsole(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.sh") + " " + serverUpdateArguments);
                    steamCMDProcess.StartInfo.FileName = Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.sh");
                    steamCMDProcess.Start();
                }

                int outputTime = 0;
                Task task = ConsumeOutput(steamCMDProcess.StandardOutput, s =>
                {
                    if (s != null)
                    {
                        WriteToConsole(s);
                        if ( outputTime != -1 && s.Contains("Steam Guard"))
                        {
                            props._serverStatus = "Steam Guard";
                        }
                        if (s.Contains("Waiting for client config"))
                        {
                            outputTime = -1;
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
                    if (outputTime > 15 && props._serverStatus != "Steam Guard")
                    {
                        WriteToConsole("Steam Guard");
                        props._serverStatus = "Steam Guard";
                        outputTime = -1;
                    }
                    if (outputTime != -1)
                    {
                        outputTime++;

                    }
                }

                steamCMDProcess.WaitForExit();
                props._serverStatus = "Server downloaded";
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
                props._serverStatus = "Error";
            }
        }

        private async Task ConsumeOutput(TextReader reader, Action<string> callback)
        {
            char[] buffer = new char[256];
            int cch;

            while ((cch = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                callback(new string(buffer, 0, cch));
            }
        }

        public string WriteSteamGuard(string code)
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
                    return "No SteamCMD Process";
                }

                return "Server was downloaded";
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
                return "Error";
            }
        }

        private DateTime GetDateBeforeUpdate()
        {
            try
            {
                if (FileSystem.FileExists(Path.Combine(Manager.SERVER_PATH, "DayZServer_x64.exe")))
                {
                    return File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_PATH, "DayZServer_x64.exe"));
                }
                else if (FileSystem.FileExists(Path.Combine(Manager.SERVER_PATH, "DayZServer")))
                {
                    return File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_PATH, "DayZServer"));
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            catch (System.Exception ex)
            {
                WriteToConsole(ex.ToString());
                return DateTime.MinValue;
            }
        }

        private void CheckForUpdatedServer(DateTime dateBeforeUpdate)
        {
            try
            {
                DateTime dateAfterUpdate;
                if (FileSystem.FileExists(Path.Combine(Manager.SERVER_PATH, "DayZServer_x64.exe")))
                {
                    dateAfterUpdate = File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_PATH, "DayZServer_x64.exe"));
                }
                else if (FileSystem.FileExists(Path.Combine(Manager.SERVER_PATH, "DayZServer")))
                {
                    dateAfterUpdate = File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_PATH, "DayZServer"));
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

        public bool CheckSteamCMD()
        {
            return steamCMDProcess != null && !steamCMDProcess.HasExited;
        }

        public void UpdateAndMoveMods(ManagerProps props, bool hasToUpdate, bool hasToMove)
        {
            List<Mod> mods = new List<Mod>();
            mods.AddRange(config.clientMods);
            mods.AddRange(config.serverMods);
            if (mods.Count > 0)
            {
                if (hasToUpdate)
                {
                    WriteToConsole("Updating Mods");
                    UpdateMods(props, mods);
                    WriteToConsole("Mods updated");
                }
                if (hasToMove)
                {
                    WriteToConsole("Moving Mods");
                    MoveMods(mods);
                    WriteToConsole("Mods moved");
                }

                List<Mod> expansionMods = config.clientMods.FindAll(x => x.name.Contains("expansion") || x.name.Contains("Expansion"));

                if (updatedServer || (expansionMods.Count > 0 && expansionMods.FindAll(mod => updatedModsIDs.Contains(mod.workshopID)).Count > 0))
                {
                    updatedServer = false;
                    WriteToConsole($"Updating Mission folder");
                    MissionUpdater upd = new MissionUpdater(config);
                    upd.Update();
                    WriteToConsole($"Finished updating Mission folder");
                }
            }
        }

        public void UpdateMods(ManagerProps props, List<Mod> mods)
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
                    string arguments = $"+force_install_dir {Path.Combine("..", Manager.MODS_PATH)} +login {config.steamUsername} {config.steamPassword}{modUpdateArguments} +quit";

                    StartSteamCMD(props, arguments);

                    WriteToConsole($"All mods were downloaded");

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
            foreach (long key in updatedModsIDs)
            {
                try
                {
                    Mod? mod = SearchForMod(key, mods);
                    if (mod != null)
                    {
                        string steamModPath = Path.Combine(Manager.MODS_PATH, Manager.WORKSHOP_PATH, mod.workshopID.ToString());
                        string serverModPath = Path.Combine(Manager.SERVER_PATH, mod.name);

                        WriteToConsole($"Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                        if (FileSystem.DirectoryExists(steamModPath))
                        {
                            FileSystem.CopyDirectory(steamModPath, serverModPath, true);

                            string serverKeysPath = GetKeysFolder(Manager.SERVER_PATH);
                            string modKeysPath = GetKeysFolder(serverModPath);

                            if (modKeysPath != string.Empty && serverKeysPath != string.Empty && FileSystem.DirectoryExists(modKeysPath) && FileSystem.DirectoryExists(serverKeysPath))
                            {
                                FileSystem.CopyDirectory(modKeysPath, serverKeysPath, true);
                                }
                            WriteToConsole($"Mod was moved to {mod.name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteToConsole(ex.ToString());
                }
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
                    SchedulerFile? schedulerFile = XMLSerializer.DeserializeXMLFile<SchedulerFile>(Path.Combine(Manager.BEC_PATH, "Config", "configUpdate.xml"));

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

                        XMLSerializer.SerializeXMLFile<SchedulerFile>(Path.Combine(Manager.BEC_PATH, "Config", "SchedulerUpdate.xml"), schedulerFile);

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        if (OperatingSystem.IsWindows())
                        {
                            startInfo.FileName = Path.Combine(Manager.BEC_PATH, "Bec.exe");
                        }
                        else
                        {
                            startInfo.FileName = Path.Combine(Manager.BEC_PATH, "Bec");
                        }
                        startInfo.Arguments = $"-f ConfigUpdate.cfg --dsc";
                        startInfo.WorkingDirectory = Manager.BEC_PATH;
                        becUpdateProcess = new Process();
                        becUpdateProcess.StartInfo = startInfo;
                        if (OperatingSystem.IsWindows())
                        {
                            WriteToConsole("Starting BEC for mod updates");
                            becUpdateProcess.Start();
                            WriteToConsole("BEC for mod updates started");
                        }
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
                string dataPath = Path.Combine(Manager.SERVER_PATH, "mpmissions", config.missionName, "storage_1");
                string profilePath = Path.Combine(Manager.SERVER_PATH, config.profileName);
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
                            FileSystem.MoveFile(fileName, Path.Combine(newestBackupPath, "logs", fileName.Substring(fileName.LastIndexOf(folderSeparator) + 1)));
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
                    HttpResponseMessage response = new HttpResponseMessage();
                    if (OperatingSystem.IsWindows())
                    {
                        response = client.GetAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip").Result;

                    }
                    else
                    {
                        response = client.GetAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz").Result;

                    }

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
                                using (FileStream outputFileStream = File.Create(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.tar")))
                                {
                                    using (var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                                    {
                                        decompressor.CopyTo(outputFileStream);
                                    }
                                }
                            }
                            if (File.Exists(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.tar")))
                            {
                                TarFile.ExtractToDirectory(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.tar"), Manager.STEAM_CMD_PATH, true);
                            }
                        }
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
                if (Directory.Exists(folderPath))
                {
                    List<string> subFolders = FileSystem.GetDirectories(folderPath).ToList<string>();
                    foreach (string subFolder in subFolders)
                    {
                        string folderName = subFolder.Substring(subFolder.LastIndexOf(folderSeparator) + 1);
                        if (folderName == "keys")
                        {
                            return subFolder;
                        }
                        else if (folderName == "Keys")
                        {
                            return subFolder;
                        }
                        else if (folderName == "key")
                        {
                            return subFolder;
                        }
                        else if (folderName == "Key")
                        {
                            return subFolder;
                        }
                    }
                }
                return string.Empty;
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
                string serverDirectoryPath = Path.Combine(serverModPath, steamDirectoryPath.Substring(steamDirectoryPath.LastIndexOf(folderSeparator) + 1));
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
                string serverFilePath = Path.Combine(serverModPath, steamFilePath.Substring(steamFilePath.LastIndexOf(folderSeparator) + 1));
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
    }
}