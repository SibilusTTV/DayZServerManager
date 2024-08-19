using DayZScheduler.Classes.SerializationClasses.SchedulerConfigClasses;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;

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
        Process? schedulerProcess;
        Process? schedulerUpdateProcess;
        Process? steamCMDProcess;

        public Server(ManagerConfig config)
        {
            this.config = config;
            dayZServerBranch = 223350;
            dayZGameBranch = 221100;
            serverProcess = null;
            schedulerProcess = null;
            schedulerUpdateProcess = null;
            updatedModsIDs = new List<long>();
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
                Manager.WriteToConsole(ex.ToString());
                serverProcess = null;
                return false;
            }
        }

        public bool CheckScheduler()
        {
            try
            {
                if (schedulerProcess != null && !restartingForUpdates)
                {
                    if (schedulerProcess.HasExited)
                    {
                        schedulerProcess = null;
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
                Manager.WriteToConsole(ex.ToString());
                schedulerProcess = null;
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
                    serverModsToLoad += serverMod.name + ";";
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
                procInf.FileName = Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE);
                serverProcess.StartInfo = procInf;
                Manager.WriteToConsole($"Starting Server");
                serverProcess.Start();
                Manager.WriteToConsole($"Server starting at {Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE)} with the parameters {startParameters}");
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        private string GetServerStartParameters(string clientModsToLoad, string serverModsToLoad)
        {
            string parameters = $"-instanceId={config.instanceId} \"-config={config.serverConfigName}\" \"-profiles={config.profileName}\" -port={config.port} {clientModsToLoad} {serverModsToLoad} -cpuCount={config.cpuCount}";

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

        public void UpdateScheduler()
        {
            try
            {
                if (!FileSystem.DirectoryExists(Manager.SCHEDULER_PATH))
                {
                    FileSystem.CreateDirectory(Manager.SCHEDULER_PATH);
                }

                HttpResponseMessage response;
                using (HttpClient client = new HttpClient())
                {
                    response = client.GetAsync($"{Manager.SCHEDULER_DOWNLOAD_URL}{Manager.SCHEDULER_ZIP_NAME}").Result;
                }

                if (response.IsSuccessStatusCode)
                {
                    byte[] content = response.Content.ReadAsByteArrayAsync().Result;
                    string zipPath = Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_ZIP_NAME);
                    File.WriteAllBytes(zipPath, content);
                    ZipFile.ExtractToDirectory(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_ZIP_NAME), Manager.SCHEDULER_PATH, true);
                }

                if (OperatingSystem.IsLinux())
                {
                    ProcessStartInfo procInf = new ProcessStartInfo("chmod", $"+x {Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_EXECUTABLE)}");
                    Process chmodProcess = new Process();
                    chmodProcess.StartInfo = procInf;
                    Manager.WriteToConsole("Giving executable rights to the scheduler");
                    chmodProcess.Start();
                    chmodProcess.WaitForExit();
                    Manager.WriteToConsole("Executable rights successfully given to the scheduler");
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }

            try
            {
                string battleyeParentFolder = "";

                if (OperatingSystem.IsWindows())
                {
                    battleyeParentFolder = Path.Combine(Manager.SERVER_PATH, config.profileName);
                }
                else
                {
                    battleyeParentFolder = Manager.SERVER_PATH;
                }

                List<string> schedulerDirectories = FileSystem.GetDirectories(Manager.SCHEDULER_PATH).ToList<string>();
                if (schedulerDirectories.Find(x => Path.GetFileName(x) == Manager.SCHEDULER_CONFIG_FOLDER) == null)
                {
                    FileSystem.CreateDirectory(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_FOLDER));
                }

                SchedulerConfig? schedulerConfig = JSONSerializer.DeserializeJSONFile<SchedulerConfig>(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_FOLDER, Manager.SCHEDULER_CONFIG_NAME));
                if (schedulerConfig == null)
                {
                    schedulerConfig = new SchedulerConfig();
                    schedulerConfig.IP = "127.0.0.1";
                    schedulerConfig.Port = config.RConPort;
                    schedulerConfig.Password = config.RConPassword;
                    schedulerConfig.Interval = config.restartInterval;
                    schedulerConfig.OnlyRestarts = config.clientMods.FindAll(mod => mod.name.ToLower().Contains("expansion")).Count > 0;
                    schedulerConfig.Scheduler = Manager.SCHEDULER_FILE_NAME;
                    schedulerConfig.IsOnUpdate = false;
                    schedulerConfig.BePath = Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME);
                    schedulerConfig.CustomMessages = new List<JobItem>();
                    int id = 0;
                    foreach (CustomMessage item in config.customMessages)
                    {
                        schedulerConfig.CustomMessages.Add(new JobItem(id, item.IsTimeOfDay, item.WaitTime, item.Interval, 0, item.Message));
                        id++;
                    }
                }

                JSONSerializer.SerializeJSONFile<SchedulerConfig>(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_FOLDER, Manager.SCHEDULER_CONFIG_NAME), schedulerConfig);

                schedulerConfig.Scheduler = Manager.SCHEDULER_FILE_UPDATE_NAME;
                schedulerConfig.IsOnUpdate = true;
                schedulerConfig.CustomMessages = new List<JobItem>();

                JSONSerializer.SerializeJSONFile<SchedulerConfig>(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_FOLDER, Manager.SCHEDULER_CONFIG_UPDATE_NAME), schedulerConfig);

                if (config.clientMods.FindAll(mod => mod.name.ToLower().Contains("expansion")).Count > 0)
                {
                    List<string> serverRootDirectories = FileSystem.GetDirectories(Manager.SERVER_PATH).ToList<string>();
                    if (serverRootDirectories.Find(x => Path.GetFileName(x) == config.profileName) == null)
                    {
                        FileSystem.CreateDirectory(Path.Combine(Manager.SERVER_PATH, config.profileName));
                    }

                    List<string> profileDirectories = FileSystem.GetDirectories(Path.Combine(Manager.SERVER_PATH, config.profileName)).ToList<string>();
                    if (profileDirectories.Find(x => Path.GetFileName(x) == "ExpansionMod") == null)
                    {
                        FileSystem.CreateDirectory(Path.Combine(Manager.SERVER_PATH, config.profileName, "ExpansionMod"));
                    }

                    List<string> expansionDirectories = FileSystem.GetDirectories(Path.Combine(Manager.SERVER_PATH, config.profileName, "ExpansionMod")).ToList<string>();
                    if (expansionDirectories.Find(x => Path.GetFileName(x) == "Settings") == null)
                    {
                        FileSystem.CreateDirectory(Path.Combine(Manager.SERVER_PATH, config.profileName, "ExpansionMod", "Settings"));
                    }

                    NotificationSchedulerFile? notFile = JSONSerializer.DeserializeJSONFile<NotificationSchedulerFile>(Path.Combine(Manager.SERVER_PATH, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"));
                    if (notFile == null)
                    {
                        notFile = new NotificationSchedulerFile();
                    }
                    RestartUpdater.UpdateExpansionScheduler(config, notFile);
                    JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SERVER_PATH, config.profileName, "ExpansionMod", "Settings", "NotificationSchedulerSettings.json"), notFile);
                }

                List<string> serverDirectories = FileSystem.GetDirectories(battleyeParentFolder).ToList<string>();
                if (serverDirectories.Find(x => Path.GetFileName(x) == Manager.BATTLEYE_FOLDER_NAME) == null)
                {
                    FileSystem.CreateDirectory(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME));
                }

                List<string> battleyeDirectories = FileSystem.GetDirectories(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME)).ToList<string>();
                if (battleyeDirectories.Find(x => Path.GetFileName(x) == Manager.BATTLEYE_CONFIG_NAME) == null)
                {
                    CreateAndSaveNewBeConfig(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_CONFIG_NAME));
                }

                if (battleyeDirectories.Find(x => Path.GetFileName(x) == Manager.BATTLEYE_BANS_NAME) == null)
                {
                    CreateAndSaveNewBans(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_BANS_NAME));
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        public void StartScheduler()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_EXECUTABLE);
                startInfo.Arguments = $"-config Config.json";
                startInfo.WorkingDirectory = Manager.SCHEDULER_PATH;
                schedulerProcess = new Process();
                schedulerProcess.StartInfo = startInfo;
                Manager.WriteToConsole("Starting Scheduler");
                schedulerProcess.Start();
                Manager.WriteToConsole("Scheduler started");

                Task task = ConsumeOutput(schedulerProcess.StandardOutput, s =>
                {
                    if (s != null)
                    {
                        Manager.WriteToConsole(s);
                    }
                });
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
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
                Manager.WriteToConsole(ex.ToString());
                serverProcess = null;
            }

            try
            {
                if (schedulerProcess != null)
                {
                    schedulerProcess.Kill();
                    schedulerProcess = null;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                schedulerProcess = null;
            }

            try
            {
                if (schedulerUpdateProcess != null)
                {
                    schedulerUpdateProcess.Kill();
                    schedulerUpdateProcess = null;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                schedulerUpdateProcess = null;
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
                Manager.WriteToConsole(ex.ToString());
                steamCMDProcess = null;
            }
        }

        public void UpdateAndMoveServer(ManagerProps props, bool hasToUpdate, bool hasToMove)
        {
            if (hasToUpdate)
            {
                Manager.WriteToConsole("Updating server");
                UpdateServer(props);
                Manager.WriteToConsole("Server updated");
            }
            if (hasToMove)
            {
                Manager.WriteToConsole("Moving server");
                MoveServer();
                Manager.WriteToConsole("Server moved");

                UpdateMission();
            }
        }

        private void MoveServer()
        {
            if (updatedServer)
            {
                List<string> serverDeployDirectories = FileSystem.GetDirectories(Manager.SERVER_DEPLOY).ToList<string>();
                List<string> serverDeployFiles = FileSystem.GetFiles(Manager.SERVER_DEPLOY).ToList<string>();

                List<string> filteredDirectories = serverDeployDirectories.FindAll(x => Path.GetFileName(x) != "Profiles" && Path.GetFileName(x) != "battleye");
                List<string> filteredFiles = serverDeployFiles.FindAll(x => Path.GetFileName(x) != "bans.txt" && Path.GetFileName(x) != "bans.txt" && Path.GetFileName(x) != "serverDZ.cfg" && Path.GetFileName(x) != "whitelist.txt" && Path.GetFileName(x) != "dayzsetting.xml");

                foreach (string dir in serverDeployDirectories)
                {
                    try
                    {
                        FileSystem.CopyDirectory(dir, Path.Combine(Manager.SERVER_PATH, Path.GetFileName(dir)), true);
                    }
                    catch (Exception ex)
                    {
                        Manager.WriteToConsole(ex.ToString());
                    }
                }

                foreach (string file in serverDeployFiles)
                {
                    try
                    {
                        FileSystem.CopyFile(file, Path.Combine(Manager.SERVER_DEPLOY, Path.GetFileName(file)), true);
                    }
                    catch (Exception ex)
                    {
                        Manager.WriteToConsole(ex.ToString());
                    }
                }
            }
        }

        private void UpdateServer(ManagerProps props)
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
                Manager.WriteToConsole(ex.ToString());
            }

            try
            {
                if (!FileSystem.FileExists(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE)))
                {
                    DownloadAndExctractSteamCMD(Manager.STEAM_CMD_ZIP_NAME);
                }
            }
            catch (System.Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }

            try
            {
                string serverUpdateArguments = $"+force_install_dir {Path.Combine("..", Manager.SERVER_DEPLOY)} \"+login {config.steamUsername} {config.steamPassword}\" \"+app_update {dayZServerBranch}\" +quit";
                Manager.WriteToConsole("Updating the DayZ Server");
                StartSteamCMD(props, serverUpdateArguments);
                if (props._serverStatus == "Server downloaded")
                {
                    CheckForUpdatedServer();
                }
            }
            catch (System.Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                props._serverStatus = "Error";
            }
        }

        private void UpdateMission()
        {
            List<Mod> expansionMods = config.clientMods.FindAll(x => x.name.ToLower().Contains("expansion"));

            if (updatedServer || (expansionMods.Count > 0 && expansionMods.FindAll(mod => updatedModsIDs.Contains(mod.workshopID)).Count > 0))
            {
                updatedServer = false;
                Manager.WriteToConsole($"Updating Mission folder");
                MissionUpdater upd = new MissionUpdater(config);
                upd.Update();
                Manager.WriteToConsole($"Finished updating Mission folder");
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
                Manager.WriteToConsole(Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE) + " " + serverUpdateArguments);
                steamCMDProcess.StartInfo.FileName = Path.Combine(Manager.STEAM_CMD_PATH, Manager.STEAM_CMD_EXECUTABLE);
                steamCMDProcess.Start();

                int outputTime = 0;
                Task task = ConsumeOutput(steamCMDProcess.StandardOutput, s =>
                {
                    if (s != null)
                    {
                        Manager.WriteToConsole(s);
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
                        Manager.WriteToConsole("Steam Guard");
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
                Manager.WriteToConsole(ex.ToString());
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
                Manager.WriteToConsole(ex.ToString());
                return "Error";
            }
        }

        private void CheckForUpdatedServer()
        {
            try
            {
                DateTime dateBeforeUpdate;
                List<string> serverDeployDirectories = FileSystem.GetFiles(Manager.SERVER_PATH).ToList<string>();
                if (serverDeployDirectories.Find(x => Path.GetFileName(x) == Manager.SERVER_EXECUTABLE) != null)
                {
                    dateBeforeUpdate = File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE));
                }
                else
                {
                    dateBeforeUpdate = DateTime.MinValue;
                }

                DateTime dateAfterUpdate;
                List<string> serverDirectories = FileSystem.GetFiles(Manager.SERVER_DEPLOY).ToList<string>();
                if (serverDirectories.Find(x => Path.GetFileName(x) == Manager.SERVER_EXECUTABLE) != null)
                {
                    dateAfterUpdate = File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_DEPLOY, Manager.SERVER_EXECUTABLE));
                }
                else
                {
                    dateAfterUpdate = DateTime.MinValue;
                }

                if (dateBeforeUpdate < dateAfterUpdate)
                {
                    updatedServer = true;
                    Manager.WriteToConsole("DayZ Server updated");
                }
                else
                {
                    updatedServer = false;
                    Manager.WriteToConsole("Server was already up-to-date");
                }
            }
            catch (System.Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
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
                    Manager.WriteToConsole("Updating Mods");
                    UpdateMods(props, mods);
                    Manager.WriteToConsole("Mods updated");
                }
                if (hasToMove)
                {
                    Manager.WriteToConsole("Moving Mods");
                    MoveMods(mods);
                    Manager.WriteToConsole("Mods moved");

                    UpdateMission();
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
                            Manager.WriteToConsole($"{mod.name} was updated");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
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

                        Manager.WriteToConsole($"Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                        if (FileSystem.DirectoryExists(steamModPath))
                        {
                            FileSystem.CopyDirectory(steamModPath, serverModPath, true);

                            string serverKeysPath = GetKeysFolder(Manager.SERVER_PATH);
                            string modKeysPath = GetKeysFolder(serverModPath);

                            if (modKeysPath != string.Empty && serverKeysPath != string.Empty && FileSystem.DirectoryExists(modKeysPath) && FileSystem.DirectoryExists(serverKeysPath))
                            {
                                FileSystem.CopyDirectory(modKeysPath, serverKeysPath, true);
                                }
                            Manager.WriteToConsole($"Mod was moved to {mod.name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
                }
            }
        }

        public bool RestartForUpdates()
        {
            if (config.restartOnUpdate && !restartingForUpdates && ((updatedMods && updatedModsIDs != null && updatedModsIDs.Count > 0) || updatedServer) && !(schedulerUpdateProcess != null && schedulerUpdateProcess.HasExited))
            {
                updatedMods = false;
                updatedServer = false;
                try
                {
                    if (IsTimeToRestart(config.restartInterval))
                    {
                        restartingForUpdates = true;
                        if (schedulerProcess != null && !schedulerProcess.HasExited)
                        {
                            schedulerProcess.Kill();
                        }

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.FileName = Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_EXECUTABLE);
                        startInfo.Arguments = $"-config ConfigUpdate.cfg";
                        startInfo.WorkingDirectory = Manager.SCHEDULER_PATH;
                        schedulerUpdateProcess = new Process();
                        schedulerUpdateProcess.StartInfo = startInfo;
                        Manager.WriteToConsole("Starting Scheduler for mod updates");
                        schedulerUpdateProcess.Start();
                        Manager.WriteToConsole("Scheduler for mod updates started");
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
            else
            {
                updatedMods = false;
                updatedServer = false;
            }
            return false;
        }

        public static bool IsTimeToRestart(int interval)
        {
            DateTime currentTime = DateTime.Now;
            if (currentTime.Hour % interval == interval - 1)
            {
                if ((currentTime.Minute >= 0 && currentTime.Minute < 5)
                    || (currentTime.Minute >= 5 && currentTime.Minute < 15))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if ((currentTime.Minute >= 50 && currentTime.Minute < 60)
                    || (currentTime.Minute >= 0 && currentTime.Minute < 5)
                    || (currentTime.Minute >= 5 && currentTime.Minute < 20)
                    || (currentTime.Minute >= 20 && currentTime.Minute < 35)
                    || (currentTime.Minute >= 35 && currentTime.Minute < 50))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
                Manager.WriteToConsole(ex.ToString());
            }

            try
            {
                Manager.WriteToConsole($"Backing up the server data and moving all the logs!");
                string newestBackupPath = Path.Combine(config.backupPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                string dataPath = Path.Combine(Manager.MISSION_PATH, config.missionName, "storage_1");
                string profilePath = Path.Combine(Manager.SERVER_PATH, config.profileName);
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
                                using (FileStream outputFileStream = File.Create(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.tar")))
                                {
                                    using (var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                                    {
                                        decompressor.CopyTo(outputFileStream);
                                    }
                                }
                            }
                            List<string> steamCMDDirectories = FileSystem.GetFiles(Manager.STEAM_CMD_PATH).ToList<string>();
                            if (steamCMDDirectories.Find(x => Path.GetFileName(x) == "steamcmd.tar") != null)
                            {
                                TarFile.ExtractToDirectory(Path.Combine(Manager.STEAM_CMD_PATH, "steamcmd.tar"), Manager.STEAM_CMD_PATH, true);
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

        private string GetKeysFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    List<string> subFolders = FileSystem.GetDirectories(folderPath).ToList<string>();
                    foreach (string subFolder in subFolders)
                    {
                        string folderName = Path.GetFileName(subFolder);
                        if (folderName.ToLower() == "keys" || folderName.ToLower() == "key")
                        {
                            return subFolder;
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                return string.Empty;
            }
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
                string serverDirectoryPath = Path.Combine(serverModPath, Path.GetFileName(steamDirectoryPath));
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
                Manager.WriteToConsole(ex.ToString());
                return false;
            }
        }

        private bool CheckFile(string steamFilePath, string serverModPath)
        {
            try
            {
                string serverFilePath = Path.Combine(serverModPath, Path.GetFileName(steamFilePath));
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
                Manager.WriteToConsole(ex.ToString());
                return false;
            }
        }

        private void CreateAndSaveNewBeConfig(string path)
        {
            try
            {
                string beConfig = $"RConPassword {config.RConPassword}";
                beConfig += $"{Environment.NewLine}RConPort {config.RConPort}";
                beConfig += $"{Environment.NewLine}RestrictRCon 0";

                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(beConfig);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
            }
        }

        private void CreateAndSaveNewBans(string path)
        {
            string bans = "";

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(bans);
                writer.Close();
            }
        }
    }
}