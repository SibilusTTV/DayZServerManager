using DayZScheduler.Classes.SerializationClasses.SchedulerConfigClasses;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DayZServerManager.Server.Classes
{
    internal class Server
    {
        // Other Variables
        private bool updatedMods;
        private bool restartingForUpdates;
        private bool updatedServer;
        private List<long> updatedModsIDs;

        Process? serverProcess;
        Process? schedulerProcess;
        Process? schedulerUpdateProcess;
        Process? steamCMDProcess;

        public Server()
        {
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
                    if (Manager.props != null)
                    {
                        Manager.props.dayzServerStatus = Manager.STATUS_RUNNING;
                    }
                    return true;
                }
                else
                {
                    if (Manager.props != null)
                    {
                        Manager.props.dayzServerStatus = Manager.STATUS_NOT_RUNNING;
                    }
                    serverProcess = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (Manager.props != null)
                {
                    Manager.props.dayzServerStatus = Manager.STATUS_NOT_RUNNING;
                }
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
            if (Manager.managerConfig != null && Manager.props != null)
            {
                updatedModsIDs = new List<long>();
                updatedMods = false;
                restartingForUpdates = false;
                updatedServer = false;
                string clientModsToLoad = string.Empty;
                foreach (Mod clientMod in Manager.managerConfig.clientMods)
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
                foreach (Mod serverMod in Manager.managerConfig.serverMods)
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
                    Manager.WriteToConsole(Manager.STATUS_STARTING_SERVER);
                    serverProcess.Start();
                    Manager.props.dayzServerStatus = Manager.STATUS_RUNNING;
                    Manager.WriteToConsole($"Server starting at {Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE)} with the parameters {startParameters}");
                }
                catch (Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
                }
            }
        }

        private string GetServerStartParameters(string clientModsToLoad, string serverModsToLoad)
        {
            string parameters = "";
            if (Manager.managerConfig != null)
            {
                parameters = $"-instanceId={Manager.managerConfig.instanceId} \"-config={Manager.managerConfig.serverConfigName}\" \"-profiles={Manager.managerConfig.profileName}\" -port={Manager.managerConfig.serverPort} {clientModsToLoad} {serverModsToLoad} -cpuCount={Manager.managerConfig.cpuCount}";

                if (Manager.managerConfig.noFilePatching)
                {
                    parameters += " -noFilePatching";
                }
                if (Manager.managerConfig.doLogs)
                {
                    parameters += " -doLogs";
                }
                if (Manager.managerConfig.adminLog)
                {
                    parameters += " -adminLog";
                }
                if (Manager.managerConfig.freezeCheck)
                {
                    parameters += " -freezeCheck";
                }
                if (Manager.managerConfig.netLog)
                {
                    parameters += " -netLog";
                }
                if (Manager.managerConfig.limitFPS > 0)
                {
                    parameters += $" -limitFPS={Manager.managerConfig.limitFPS}";
                }
            }

            return parameters;
        }

        public void UpdateScheduler()
        {
            if (Manager.managerConfig != null)
            {
                try
                {
                    if (Manager.props != null) Manager.props.managerStatus = Manager.STATUS_UPDATING_SCHEDULER;
                    Manager.WriteToConsole(Manager.STATUS_UPDATING_SCHEDULER);

                    if (!Directory.Exists(Manager.SCHEDULER_PATH))
                    {
                        Directory.CreateDirectory(Manager.SCHEDULER_PATH);
                    }


                    Manager.WriteToConsole("Downloading Scheduler");
                    HttpResponseMessage response;
                    using (HttpClient client = new HttpClient())
                    {
                        response = client.GetAsync($"{Manager.SCHEDULER_DOWNLOAD_URL}{Manager.SCHEDULER_ZIP_NAME}").Result;
                    }
                    Manager.WriteToConsole("Scheduler Downloaded");

                    Manager.WriteToConsole("Unpacking Zip");
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] content = response.Content.ReadAsByteArrayAsync().Result;
                        string zipPath = Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_ZIP_NAME);
                        File.WriteAllBytes(zipPath, content);
                        ZipFile.ExtractToDirectory(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_ZIP_NAME), Manager.SCHEDULER_PATH, true);
                    }
                    Manager.WriteToConsole("Zip Unpacked");

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
                        battleyeParentFolder = Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName);
                    }
                    else
                    {
                        battleyeParentFolder = Manager.SERVER_PATH;
                    }

                    if (!Directory.Exists(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_FOLDER)))
                    {
                        Directory.CreateDirectory(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_FOLDER));
                    }

                    SchedulerConfig? schedulerConfig = JSONSerializer.DeserializeJSONFile<SchedulerConfig>(Path.Combine(Manager.SCHEDULER_PATH, Manager.SCHEDULER_CONFIG_FOLDER, Manager.SCHEDULER_CONFIG_NAME));
                    if (schedulerConfig == null)
                    {
                        schedulerConfig = new SchedulerConfig();
                        schedulerConfig.IP = "127.0.0.1";
                        schedulerConfig.Port = Manager.managerConfig.RConPort;
                        schedulerConfig.Password = Manager.managerConfig.RConPassword;
                        schedulerConfig.Interval = Manager.managerConfig.restartInterval;
                        schedulerConfig.OnlyRestarts = Manager.managerConfig.clientMods.FindAll(mod => mod.name.ToLower().Contains(Manager.EXPANSION_MOD_SEARCH)).Count > 0;
                        schedulerConfig.Scheduler = Manager.SCHEDULER_FILE_NAME;
                        schedulerConfig.IsOnUpdate = false;
                        schedulerConfig.BePath = Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME);
                        schedulerConfig.CustomMessages = new List<JobItem>();
                        int id = 0;
                        foreach (CustomMessage item in Manager.managerConfig.customMessages)
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

                    if (Manager.managerConfig.clientMods.FindAll(mod => mod.name.ToLower().Contains(Manager.EXPANSION_MOD_SEARCH)).Count > 0)
                    {
                        if (!Directory.Exists(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName)))
                        {
                            Directory.CreateDirectory(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName));
                        }

                        if (!Directory.Exists(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.PROFILE_EXPANSIONMOD_FOLDER_NAME)))
                        {
                            Directory.CreateDirectory(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.PROFILE_EXPANSIONMOD_FOLDER_NAME));
                        }

                        if (!Directory.Exists(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.PROFILE_EXPANSIONMOD_FOLDER_NAME, Manager.PROFILE_EXPANSION_SETTINGS_FOLDER_NAME)))
                        {
                            Directory.CreateDirectory(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.PROFILE_EXPANSIONMOD_FOLDER_NAME, Manager.PROFILE_EXPANSION_SETTINGS_FOLDER_NAME));
                        }

                        NotificationSchedulerFile? notFile = JSONSerializer.DeserializeJSONFile<NotificationSchedulerFile>(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.PROFILE_EXPANSIONMOD_FOLDER_NAME, Manager.PROFILE_EXPANSION_SETTINGS_FOLDER_NAME, Manager.PROFILE_EXPANSION_NOTIFICATION_SCHEDULER_SETTINGS_FILE_NAME));
                        if (notFile == null)
                        {
                            notFile = new NotificationSchedulerFile();
                        }
                        RestartUpdater.UpdateExpansionScheduler(Manager.managerConfig, notFile);
                        JSONSerializer.SerializeJSONFile(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.PROFILE_EXPANSIONMOD_FOLDER_NAME, Manager.PROFILE_EXPANSION_SETTINGS_FOLDER_NAME, Manager.PROFILE_EXPANSION_NOTIFICATION_SCHEDULER_SETTINGS_FILE_NAME), notFile);
                    }

                    if (!Directory.Exists(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME)))
                    {
                        Directory.CreateDirectory(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME));
                    }

                    if (!File.Exists(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_CONFIG_NAME)))
                    {
                        CreateAndSaveNewBeConfig(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_CONFIG_NAME));
                    }

                    if (File.Exists(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_BANS_NAME)))
                    {
                        CreateAndSaveNewBans(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME, Manager.BATTLEYE_BANS_NAME));
                    }

                    Manager.WriteToConsole(Manager.STATUS_SCHEDULER_UPDATED);
                    if (Manager.props != null) Manager.props.managerStatus = Manager.STATUS_SCHEDULER_UPDATED;
                }
                catch (Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
                }
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
                startInfo.Arguments = $"-config {Manager.SCHEDULER_CONFIG_NAME}";
                startInfo.WorkingDirectory = Manager.SCHEDULER_PATH;
                schedulerProcess = new Process();
                schedulerProcess.StartInfo = startInfo;
                Manager.WriteToConsole(Manager.STATUS_STARTING_SCHEDULER);
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
                if (serverProcess != null && Manager.props != null)
                {
                    serverProcess.Kill();
                    serverProcess = null;
                    Manager.props.dayzServerStatus = Manager.STATUS_NOT_RUNNING;
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

        public void UpdateAndMoveServer(ManagerProps props, bool hasToUpdate, bool hasToMove)
        {
            if (Manager.props != null)
            {
                if (hasToUpdate)
                {
                    Manager.props.managerStatus = Manager.STATUS_UPDATING_SERVER;
                    Manager.WriteToConsole(Manager.STATUS_UPDATING_SERVER);
                    UpdateServer(props);
                    Manager.props.managerStatus = Manager.STATUS_SERVER_UPDATED;
                    Manager.WriteToConsole(Manager.STATUS_SERVER_UPDATED);
                }

                if (hasToMove)
                {
                    Manager.props.managerStatus = Manager.STATUS_MOVING_SERVER;
                    Manager.WriteToConsole(Manager.STATUS_MOVING_SERVER);
                    MoveServer();
                    Manager.props.managerStatus = Manager.STATUS_SERVER_MOVED;
                    Manager.WriteToConsole(Manager.STATUS_SERVER_MOVED);

                    UpdateMission();
                }
            }
        }

        private void MoveServer()
        {
            if (updatedServer && Manager.managerConfig != null)
            {
                List<string> serverDeployDirectories = Directory.GetDirectories(Manager.SERVER_DEPLOY).ToList<string>();
                List<string> serverDeployFiles = Directory.GetFiles(Manager.SERVER_DEPLOY).ToList<string>();

                List<string> filteredDirectories = serverDeployDirectories.FindAll(x => Path.GetFileName(x) != Manager.managerConfig.profileName && Path.GetFileName(x) != Manager.BATTLEYE_FOLDER_NAME);
                List<string> filteredFiles = serverDeployFiles.FindAll(x => Path.GetFileName(x).ToLower() != Manager.BANS_FILE_NAME && Path.GetFileName(x).ToLower() != Manager.BAN_FILE_NAME && Path.GetFileName(x) != Manager.managerConfig.serverConfigName && Path.GetFileName(x).ToLower() != Manager.WHITELIST_FILE_NAME && Path.GetFileName(x).ToLower() != Manager.DAYZ_SETTINGS_FILE_NAME);

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

                foreach (string file in filteredFiles)
                {
                    try
                    {
                        File.Copy(file, Path.Combine(Manager.SERVER_PATH, Path.GetFileName(file)), true);
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
            if (Manager.managerConfig != null)
            {
                try
                {
                    if (!Directory.Exists(Manager.STEAM_CMD_PATH))
                    {
                        Directory.CreateDirectory(Manager.STEAM_CMD_PATH);
                    }
                }
                catch (System.Exception ex)
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
                catch (System.Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
                }

                try
                {
                    string serverUpdateArguments = $"+force_install_dir {Path.Combine("..", Manager.SERVER_DEPLOY)} \"+login {Manager.managerConfig.steamUsername} {Manager.managerConfig.steamPassword}\" \"+app_update {Manager.DAYZ_SERVER_BRANCH}\" +quit";
                    Manager.WriteToConsole("Updating the DayZ Server");
                    StartSteamCMD(props, serverUpdateArguments);
                    if (props.steamCMDStatus == Manager.STATUS_NOT_RUNNING)
                    {
                        CheckForUpdatedServer();
                    }
                }
                catch (System.Exception ex)
                {
                    Manager.WriteToConsole(ex.ToString());
                    props.managerStatus = Manager.STATUS_ERROR;
                }
            }
        }

        private void UpdateMission()
        {
            if (Manager.managerConfig != null && Manager.props != null)
            {
                List<Mod> expansionMods = Manager.managerConfig.clientMods.FindAll(x => x.name.ToLower().Contains(Manager.EXPANSION_MOD_SEARCH));

                if (updatedServer || (expansionMods.Count > 0 && expansionMods.FindAll(mod => updatedModsIDs.Contains(mod.workshopID)).Count > 0))
                {
                    updatedMods = false;
                    updatedServer = false;
                    Manager.WriteToConsole(Manager.STATUS_UPDATING_MISSION);
                    Manager.props.managerStatus = Manager.STATUS_UPDATING_MISSION;
                    MissionUpdater.Update();
                    Manager.WriteToConsole(Manager.STATUS_MISSION_UPDATED);
                    Manager.props.managerStatus = Manager.STATUS_MISSION_UPDATED;
                }
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

                props.steamCMDStatus = Manager.STATUS_RUNNING;

                int outputTime = 0;
                Task task = ConsumeOutput(steamCMDProcess.StandardOutput, s =>
                {
                    if (s != null)
                    {
                        Manager.WriteToConsole(s); 
                        if (s.Contains("client config"))
                        {
                            props.steamCMDStatus = Manager.STATUS_CLIENT_CONFIG;
                            outputTime = -1;
                        }
                        else if (s.Contains("Steam Guard"))
                        {
                            props.steamCMDStatus = Manager.STATUS_STEAM_GUARD;
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
                        Manager.WriteToConsole("Steam Guard");
                        props.steamCMDStatus = Manager.STATUS_STEAM_GUARD;
                        outputTime = 0;
                    }
                    else if (outputTime != -1)
                    {
                        outputTime++;
                    }
                }

                steamCMDProcess.WaitForExit();

                props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
            }
            catch (System.Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                props.dayzServerStatus = Manager.STATUS_ERROR;
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
                    if (Manager.props != null) Manager.props.steamCMDStatus = Manager.STATUS_NOT_RUNNING;
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
                if (!File.Exists(Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE)))
                {
                    dateBeforeUpdate = File.GetLastWriteTimeUtc(Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE));
                }
                else
                {
                    dateBeforeUpdate = DateTime.MinValue;
                }

                DateTime dateAfterUpdate;
                if (!File.Exists(Path.Combine(Manager.SERVER_DEPLOY, Manager.SERVER_EXECUTABLE)))
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
            if (Manager.managerConfig != null)
            {
                List<Mod> mods = new List<Mod>();
                mods.AddRange(Manager.managerConfig.clientMods);
                mods.AddRange(Manager.managerConfig.serverMods);
                if (mods.Count > 0 && Manager.props != null)
                {
                    if (hasToUpdate)
                    {
                        Manager.props.managerStatus = Manager.STATUS_UPDATING_MODS;
                        Manager.WriteToConsole(Manager.STATUS_UPDATING_MODS);
                        UpdateMods(props, mods);
                        Manager.props.managerStatus = Manager.STATUS_MODS_UPDATED;
                        Manager.WriteToConsole(Manager.STATUS_MODS_UPDATED);
                    }
                    if (hasToMove)
                    {
                        Manager.props.managerStatus = Manager.STATUS_MOVING_MODS;
                        Manager.WriteToConsole(Manager.STATUS_MOVING_MODS);
                        MoveMods(mods);
                        Manager.props.managerStatus = Manager.STATUS_MODS_MOVED;
                        Manager.WriteToConsole(Manager.STATUS_MODS_MOVED);

                        UpdateMission();
                    }
                }
            }
        }

        public void UpdateMods(ManagerProps props, List<Mod> mods)
        {
            if (Manager.managerConfig != null)
            {
                try
                {
                    string modUpdateArguments = string.Empty;
                    if (mods.Count > 0)
                    {
                        foreach (Mod mod in mods)
                        {
                            modUpdateArguments += $" +workshop_download_item {Manager.DAYZ_GAME_BRANCH} {mod.workshopID.ToString()}";
                        }
                        string arguments = $"+force_install_dir {Path.Combine("..", Manager.MODS_PATH)} +login {Manager.managerConfig.steamUsername} {Manager.managerConfig.steamPassword}{modUpdateArguments} +quit";

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
        }

        public void MoveMods(List<Mod> mods)
        {
            if (updatedMods)
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
                            if (Directory.Exists(steamModPath))
                            {
                                FileSystem.CopyDirectory(steamModPath, serverModPath, true);

                                string serverKeysPath = GetKeysFolder(Manager.SERVER_PATH);
                                string modKeysPath = GetKeysFolder(serverModPath);

                                if (modKeysPath != string.Empty && serverKeysPath != string.Empty && Directory.Exists(modKeysPath) && Directory.Exists(serverKeysPath))
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
        }

        public bool RestartForUpdates()
        {
            if (Manager.managerConfig != null && Manager.managerConfig.restartOnUpdate && !restartingForUpdates && ((updatedMods && updatedModsIDs != null && updatedModsIDs.Count > 0) || updatedServer) && !(schedulerUpdateProcess != null && schedulerUpdateProcess.HasExited))
            {
                try
                {
                    if (IsTimeToRestart(Manager.managerConfig.restartInterval))
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
                        startInfo.Arguments = $"-config {Manager.SCHEDULER_CONFIG_UPDATE_NAME}";
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

        public void BackupServerData(ManagerProps props)
        {
            props.managerStatus = Manager.STATUS_BACKING_UP_SERVER;
            Manager.WriteToConsole(Manager.STATUS_BACKING_UP_SERVER);
            if (Manager.managerConfig != null)
            {
                BackupManager.MakeBackup(Manager.managerConfig.backupPath, Manager.managerConfig.profileName, Manager.managerConfig.missionName);
                if (Manager.managerConfig.deleteBackups)
                {
                    BackupManager.DeleteOldBackups(Manager.managerConfig.backupPath, Manager.managerConfig.maxKeepTime);
                }
            }
            Manager.WriteToConsole(Manager.STATUS_SERVER_BACKED_UP);
            props.managerStatus = Manager.STATUS_SERVER_BACKED_UP;
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

        private string GetKeysFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    List<string> subFolders = Directory.GetDirectories(folderPath).ToList<string>();
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
            List<string> steamModFilePaths = Directory.GetFiles(steamModPath).ToList<string>();
            foreach (string filePath in steamModFilePaths)
            {
                if (CheckFile(filePath, serverModPath))
                {
                    return true;
                }
            }

            List<string> steamModDirectoryPaths = Directory.GetDirectories(steamModPath).ToList<string>();
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
                if (Directory.Exists(serverModPath) && Directory.Exists(serverDirectoryPath))
                {
                    List<string> steamModFilePaths = Directory.GetFiles(steamDirectoryPath).ToList<string>();
                    foreach (string filePath in steamModFilePaths)
                    {
                        if (CheckFile(filePath, serverDirectoryPath))
                        {
                            return true;
                        }
                    }

                    List<string> steamModDirectoryPaths = Directory.GetDirectories(steamDirectoryPath).ToList<string>();
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

        private void CreateAndSaveNewBeConfig(string path)
        {
            try
            {
                if (Manager.managerConfig != null)
                {
                    string beConfig = $"RConPassword {Manager.managerConfig.RConPassword}";
                    beConfig += $"{Environment.NewLine}RConPort {Manager.managerConfig.RConPort}";
                    beConfig += $"{Environment.NewLine}RestrictRCon 0";

                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        writer.Write(beConfig);
                        writer.Close();
                    }
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