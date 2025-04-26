using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Handlers.BackupHandler;
using DayZServerManager.Server.Classes.Handlers.SchedulerHandler;
using DayZServerManager.Server.Classes.Handlers.SteamCMDHandler;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using LibGit2Sharp;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DayZServerManager.Server.Classes.Handlers.ServerHandler
{
    internal class ServerManager
    {
        // Other Variables
        private bool updatedMods;
        private bool restartingForUpdates;
        private bool updatedServer;
        private List<long> updatedModsIDs;

        public SchedulerManager? scheduler;
        public SchedulerManager? schedulerUpdate;
        private Task? connectTask;

        public Process? serverProcess;


        public ServerManager()
        {
            serverProcess = null;
            scheduler = null;
            schedulerUpdate = null;
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
                if (scheduler != null && !restartingForUpdates)
                {
                    if (!scheduler.IsConnected() && (connectTask == null || connectTask.IsCompleted))
                    {
                        scheduler.KillTasks();
                        scheduler = null;
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
                scheduler = null;
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
                    if (!File.Exists(Path.Combine(Manager.SERVER_PATH, "ban.txt")))
                    {
                        File.Create(Path.Combine(Manager.SERVER_PATH, "ban.txt"));
                    }

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
                    string battleyeParentFolder = "";

                    if (OperatingSystem.IsWindows())
                    {
                        battleyeParentFolder = Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName);
                    }
                    else
                    {
                        battleyeParentFolder = Manager.SERVER_PATH;
                    }

                    if (!Directory.Exists(Manager.SCHEDULER_PATH))
                    {
                        Directory.CreateDirectory(Manager.SCHEDULER_PATH);
                    }

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

                    List<string> beFiles = FileSystem.GetFiles(Path.Combine(battleyeParentFolder, Manager.BATTLEYE_FOLDER_NAME)).ToList();
                    foreach (string beFile in beFiles)
                    {
                        if (Path.GetExtension(beFile) == ".cfg" && Path.GetFileNameWithoutExtension(beFile).Contains(Path.GetFileNameWithoutExtension(Manager.BATTLEYE_CONFIG_NAME)))
                        {
                            UpdateBeConfig(beFile);
                        }
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
                if (Manager.managerConfig != null)
                {
                    bool onlyRestarts = false;
                    List<JobItem> CustomMessages = new List<JobItem>();
                    if (Manager.managerConfig.clientMods.FindAll(mod => mod.name.ToLower().Contains(Manager.EXPANSION_MOD_SEARCH)).Count <= 0)
                    {
                        int id = 0;
                        foreach (CustomMessage item in Manager.managerConfig.customMessages)
                        {
                            CustomMessages.Add(new JobItem(id, item.IsTimeOfDay, item.WaitTime, item.Interval, 0, item.Message));
                            id++;
                        }
                    }
                    else
                    {
                        onlyRestarts = true;
                    }

                    scheduler = new SchedulerManager(Manager.LOCALHOST, Manager.managerConfig.RConPort, Manager.managerConfig.RConPassword, false, onlyRestarts, Manager.managerConfig.restartInterval, CustomMessages);
                    connectTask = new Task(() => { scheduler.Connect(); });
                    connectTask.Start();
                }
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
                if (scheduler != null)
                {
                    scheduler.KillTasks();
                    scheduler = null;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                scheduler = null;
            }

            try
            {
                if (schedulerUpdate != null)
                {
                    schedulerUpdate.KillTasks();
                    schedulerUpdate = null;
                }
            }
            catch (Exception ex)
            {
                Manager.WriteToConsole(ex.ToString());
                schedulerUpdate = null;
            }

            SteamCMDManager.KillSteamCMD();
        }

        public void UpdateAndBackupServer(ManagerProps props, bool hasToUpdate, bool hasToMove)
        {
            if (Manager.managerConfig != null)
            {
                List<Mod> mods = new List<Mod>();
                mods.AddRange(Manager.managerConfig.clientMods);
                mods.AddRange(Manager.managerConfig.serverMods);
                if (hasToUpdate)
                {
                    SteamCMDManager.UpdateServer(props);
                    if (mods.Count > 0)
                    {
                        updatedMods = SteamCMDManager.UpdateMods(props, mods, out updatedModsIDs);
                    }
                }
                if (hasToMove)
                {
                    MoveServer(props);

                    MoveMods(props, mods);

                    UpdateMission();

                    UpdateScheduler();

                    if (Manager.managerConfig != null && Manager.managerConfig.makeBackups)
                    {
                        BackupServerData(props);
                    }
                }
            }

        }

        private void MoveServer(ManagerProps props)
        {
            if (updatedServer && Manager.managerConfig != null)
            {
                props.managerStatus = Manager.STATUS_MOVING_SERVER;
                Manager.WriteToConsole(Manager.STATUS_MOVING_SERVER);

                List<string> serverDeployDirectories = Directory.GetDirectories(Manager.SERVER_DEPLOY).ToList();
                List<string> serverDeployFiles = Directory.GetFiles(Manager.SERVER_DEPLOY).ToList();

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

                props.managerStatus = Manager.STATUS_SERVER_MOVED;
                Manager.WriteToConsole(Manager.STATUS_SERVER_MOVED);
            }
        }

        public void MoveMods(ManagerProps props, List<Mod> mods)
        {
            if (updatedMods)
            {
                props.managerStatus = Manager.STATUS_MOVING_MODS;
                Manager.WriteToConsole(Manager.STATUS_MOVING_MODS);

                foreach (long key in updatedModsIDs)
                {
                    try
                    {
                        Mod? mod = mods.Find(x => x.workshopID == key);
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

                props.managerStatus = Manager.STATUS_MODS_MOVED;
                Manager.WriteToConsole(Manager.STATUS_MODS_MOVED);
            }
        }

        private void UpdateMission()
        {
            if (Manager.managerConfig != null && Manager.props != null)
            {
                List<Mod> expansionMods = Manager.managerConfig.clientMods.FindAll(x => x.name.ToLower().Contains(Manager.EXPANSION_MOD_SEARCH));

                if (updatedServer || expansionMods.Count > 0 && expansionMods.FindAll(mod => updatedModsIDs.Contains(mod.workshopID)).Count > 0)
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

        public bool RestartForUpdates()
        {
            if (Manager.managerConfig != null && Manager.managerConfig.restartOnUpdate && !restartingForUpdates && (updatedMods && updatedModsIDs != null && updatedModsIDs.Count > 0 || updatedServer) && !(schedulerUpdate != null && schedulerUpdate.IsConnected()))
            {
                try
                {
                    if (IsTimeToRestart(Manager.managerConfig.restartInterval))
                    {
                        restartingForUpdates = true;
                        if (scheduler != null)
                        {
                            scheduler.KillTasks();
                            scheduler = null;
                        }

                        List<JobItem> CustomMessages = new List<JobItem>();
                        if (Manager.managerConfig.clientMods.FindAll(mod => mod.name.ToLower().Contains(Manager.EXPANSION_MOD_SEARCH)).Count <= 0)
                        {
                            int id = 0;
                            foreach (CustomMessage item in Manager.managerConfig.customMessages)
                            {
                                CustomMessages.Add(new JobItem(id, item.IsTimeOfDay, item.WaitTime, item.Interval, 0, item.Message));
                                id++;
                            }
                        }

                        schedulerUpdate = new SchedulerManager(Manager.LOCALHOST, Manager.managerConfig.RConPort, Manager.managerConfig.RConPassword, true, false, Manager.managerConfig.restartInterval, CustomMessages);
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
                if (currentTime.Minute >= 0 && currentTime.Minute < 5
                    || currentTime.Minute >= 5 && currentTime.Minute < 15)
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
                if (currentTime.Minute >= 50 && currentTime.Minute < 60
                    || currentTime.Minute >= 0 && currentTime.Minute < 5
                    || currentTime.Minute >= 5 && currentTime.Minute < 20
                    || currentTime.Minute >= 20 && currentTime.Minute < 35
                    || currentTime.Minute >= 35 && currentTime.Minute < 50)
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

        private string GetKeysFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    List<string> subFolders = Directory.GetDirectories(folderPath).ToList();
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

        private void UpdateBeConfig(string path)
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