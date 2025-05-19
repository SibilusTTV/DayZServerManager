using DayZScheduler.Classes.SerializationClasses.SchedulerClasses;
using DayZServerManager.Server.Classes.Handlers.BackupHandler;
using DayZServerManager.Server.Classes.Handlers.RestartUpdaterHandler;
using DayZServerManager.Server.Classes.Handlers.SchedulerHandler;
using DayZServerManager.Server.Classes.Handlers.ServerHandler.MissionHandler;
using DayZServerManager.Server.Classes.Handlers.SteamCMDHandler;
using DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses.PlayersDB;
using DayZServerManager.Server.Classes.SerializationClasses.Serializers;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses.Property;
using LibGit2Sharp;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DayZServerManager.Server.Classes.Handlers.ServerHandler
{
    internal class ServerManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // Other Variables
        private bool _updatedMods;
        private bool _updatedServer;
        private bool _restartingForUpdates;
        private bool _missionNeedsUpdating;
        private List<long> _updatedModsIDs;
        private ServerConfig _serverConfig;

        private Process? _serverProcess;

        public ServerConfig ServerConfig { get { return _serverConfig; } set { _serverConfig = value; } }
        public bool RestartingForUpdates { get { return _restartingForUpdates; } }
        public bool UpdatedMods { get { return _updatedMods; } set { _updatedMods = value; } }
        public bool UpdatedServer { get { return _updatedServer; } set { _updatedServer = value; } }
        public bool MissionNeedsUpdating {  get { return _missionNeedsUpdating; } set { _missionNeedsUpdating = value; } }

        public ServerManager(string serverConfigPath)
        {
            if (!Directory.Exists(Manager.SERVER_PATH))
            {
                Directory.CreateDirectory(Manager.SERVER_PATH);
            }

            if (!File.Exists(Path.Combine(Manager.SERVER_PATH, Manager.BAN_FILE_NAME)))
            {
                File.Create(Path.Combine(Manager.SERVER_PATH, Manager.BAN_FILE_NAME));
            }

            if (!Directory.Exists(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName)))
            {
                Directory.CreateDirectory(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName));
            }

            if (!Directory.Exists(Manager.BATTLEYE_FOLDER_PATH))
            {
                Directory.CreateDirectory(Manager.BATTLEYE_FOLDER_PATH);
            }

            if (!File.Exists(Path.Combine(Manager.BATTLEYE_FOLDER_PATH, Manager.BATTLEYE_BANS_NAME)))
            {
                using (FileStream fs = File.Create(Path.Combine(Manager.BATTLEYE_FOLDER_PATH, Manager.BATTLEYE_BANS_NAME)))
                {

                }

            }

            List<string> beConfigFiles = FileSystem.GetFiles(Manager.BATTLEYE_FOLDER_PATH).ToList().FindAll(beFile => Path.GetExtension(beFile) == ".cfg" && Path.GetFileNameWithoutExtension(beFile).Contains(Path.GetFileNameWithoutExtension(Manager.BATTLEYE_CONFIG_NAME)));
            if (beConfigFiles.Count > 0)
            {
                foreach (string beConfigFile in beConfigFiles)
                {
                    UpdateBeConfig(beConfigFile);
                }
            }
            else
            {
                string beConfigPath = Path.Combine(Manager.BATTLEYE_FOLDER_PATH, Manager.BATTLEYE_CONFIG_NAME);
                using (FileStream fs = File.Create(beConfigPath))
                {

                }
                UpdateBeConfig(beConfigPath);
            }

            _serverConfig = LoadServerConfig(serverConfigPath);
            _serverProcess = null;
            _updatedModsIDs = new List<long>();
            _updatedMods = false;
            _updatedServer = false;
            _restartingForUpdates = false;
            _missionNeedsUpdating = false;
        }

        public bool CheckServer()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    Manager.props.dayzServerStatus = Manager.STATUS_RUNNING;
                    return true;
                }
                else
                {
                    Manager.props.dayzServerStatus = Manager.STATUS_NOT_RUNNING;
                    _serverProcess = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Manager.props.dayzServerStatus = Manager.STATUS_NOT_RUNNING;
                Logger.Error(ex, "Error when accessing getting server status");
                _serverProcess = null;
                return false;
            }
        }

        public void StartServer()
        {
            Manager.props.chatLog = "";

            _updatedModsIDs = new List<long>();
            _updatedMods = false;
            _restartingForUpdates = false;
            _updatedServer = false;
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
                _serverProcess = new Process();
                ProcessStartInfo procInf = new ProcessStartInfo();
                string startParameters = GetServerStartParameters(clientModsToLoad, serverModsToLoad);
                procInf.WorkingDirectory = Manager.SERVER_PATH;
                procInf.Arguments = startParameters;
                procInf.FileName = Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE);
                _serverProcess.StartInfo = procInf;
                Logger.Info(Manager.STATUS_STARTING_SERVER);
                _serverProcess.Start();
                Manager.props.dayzServerStatus = Manager.STATUS_RUNNING;
                Logger.Info($"Server starting at {Path.Combine(Manager.SERVER_PATH, Manager.SERVER_EXECUTABLE)} with the parameters {startParameters}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when starting server process");
            }
        }

        private string GetServerStartParameters(string clientModsToLoad, string serverModsToLoad)
        {
            string parameters = "";
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

            return parameters;
        }

        public void KillServerProcess()
        {
            try
            {
                if (Manager.scheduler.IsConnected())
                {
                    Manager.scheduler.Shutdown();
                    Manager.props.dayzServerStatus = Manager.STATUS_STOPPING_SERVER;
                }
                else
                {
                    if (_serverProcess != null)
                    {
                        _serverProcess.Kill();
                        _serverProcess = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when killing the server process");
                _serverProcess = null;
            }
        }

        public void UpdateAndBackupServer(bool hasToUpdate, bool hasToMove)
        {
            List<Mod> mods = new List<Mod>();
            mods.AddRange(Manager.managerConfig.clientMods);
            mods.AddRange(Manager.managerConfig.serverMods);
            if (hasToUpdate)
            {
                _updatedServer = SteamCMDManager.UpdateServer();
                if (mods.Count > 0)
                {
                    _updatedMods = SteamCMDManager.UpdateMods(mods, out _updatedModsIDs, out _missionNeedsUpdating);
                }
            }
            if (hasToMove)
            {
                MoveServer();

                MoveMods(mods);

                UpdateMission();

                Manager.UpdateExpansionNotificationFile();

                if (Manager.managerConfig.makeBackups)
                {
                    BackupServerData();
                }
            }
        }

        private void MoveServer()
        {
            if (_updatedServer)
            {
                Manager.props.managerStatus = Manager.STATUS_MOVING_SERVER;
                Logger.Info(Manager.STATUS_MOVING_SERVER);

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
                        Logger.Error(ex, "Error copying a directory");
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
                        Logger.Error(ex, "Error copying a file");
                    }
                }

                Manager.props.managerStatus = Manager.STATUS_SERVER_MOVED;
                Logger.Info(Manager.STATUS_SERVER_MOVED);
            }
        }

        public void MoveMods(List<Mod> mods)
        {
            if (_updatedMods)
            {
                Manager.props.managerStatus = Manager.STATUS_MOVING_MODS;
                Logger.Info(Manager.STATUS_MOVING_MODS);

                foreach (long key in _updatedModsIDs)
                {
                    try
                    {
                        Mod? mod = mods.Find(x => x.workshopID == key);
                        if (mod != null)
                        {
                            string steamModPath = Path.Combine(Manager.MODS_PATH, Manager.WORKSHOP_PATH, mod.workshopID.ToString());
                            string serverModPath = Path.Combine(Manager.SERVER_PATH, mod.name);

                            Logger.Info($"Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                            if (Directory.Exists(steamModPath))
                            {
                                FileSystem.CopyDirectory(steamModPath, serverModPath, true);

                                string serverKeysPath = GetKeysFolder(Manager.SERVER_PATH);
                                string modKeysPath = GetKeysFolder(serverModPath);

                                if (modKeysPath != string.Empty && serverKeysPath != string.Empty && Directory.Exists(modKeysPath) && Directory.Exists(serverKeysPath))
                                {
                                    FileSystem.CopyDirectory(modKeysPath, serverKeysPath, true);
                                }
                                Logger.Info($"Mod was moved to {mod.name}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error moving mods");
                    }
                }

                Manager.props.managerStatus = Manager.STATUS_MODS_MOVED;
                Logger.Info(Manager.STATUS_MODS_MOVED);
            }
        }

        private void UpdateMission()
        {
            if (_updatedServer || _missionNeedsUpdating)
            {
                _updatedMods = false;
                _updatedServer = false;
                _missionNeedsUpdating = false;
                Logger.Info(Manager.STATUS_UPDATING_MISSION);
                Manager.props.managerStatus = Manager.STATUS_UPDATING_MISSION;
                MissionUpdater.Update();
                Logger.Info(Manager.STATUS_MISSION_UPDATED);
                Manager.props.managerStatus = Manager.STATUS_MISSION_UPDATED;
            }
        }

        public bool RestartForUpdates()
        {
            if (Manager.managerConfig.restartOnUpdate && !_restartingForUpdates && ((_updatedMods && _updatedModsIDs != null && _updatedModsIDs.Count > 0) || _updatedServer))
            {
                try
                {
                    if (RestartUpdater.IsTimeToRestart(Manager.managerConfig.restartInterval))
                    {
                        _restartingForUpdates = true;
                        Manager.scheduler.ChangeToUpdateMode();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error when changing to update mode");
                    return false;
                }
            }
            else
            {
                _updatedMods = false;
                _updatedServer = false;
            }
            return false;
        }

        public void BackupServerData()
        {
            Manager.props.managerStatus = Manager.STATUS_BACKING_UP_SERVER;
            Logger.Info(Manager.STATUS_BACKING_UP_SERVER);
            BackupManager.MakeBackup(Manager.managerConfig.backupPath, Manager.managerConfig.profileName, Manager.managerConfig.missionName);
            if (Manager.managerConfig.deleteBackups)
            {
                BackupManager.DeleteOldBackups(Manager.managerConfig.backupPath, Manager.managerConfig.maxKeepTime);
            }
            Logger.Info(Manager.STATUS_SERVER_BACKED_UP);
            Manager.props.managerStatus = Manager.STATUS_SERVER_BACKED_UP;
        }

        public string GetAdminLog()
        {
            try
            {
                string returnString = string.Empty;
                string adminLogPath = "";
                if (OperatingSystem.IsWindows())
                {
                    adminLogPath = Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.ADMIN_LOG_X64_NAME);
                }
                else
                {
                    adminLogPath = Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.profileName, Manager.ADMIN_LOG_NAME);
                }

                if (!File.Exists(adminLogPath))
                {
                    using (FileStream fs = File.Create(adminLogPath))
                    {

                    }
                }

                using (var fs = new FileStream(adminLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.Default))
                    {
                        returnString = sr.ReadToEnd();
                    }
                }

                if (Manager.props.adminLog != returnString)
                {
                    string pattern = @"Player \""(?'name'[^\n]+)\""\(id=(?'id'\S*)\)";
                    Regex regex = new Regex(pattern);
                    MatchCollection matches = regex.Matches(returnString);

                    foreach (Match match in matches)
                    {
                        string name = match.Groups["name"].Value;
                        string uid = match.Groups["id"].Value;

                        Player? player = Manager.scheduler.PlayersDB.Players.Find(player => player.Name == name);
                        if (player != null && uid != "Unknown" && (string.IsNullOrEmpty(player.Uid) || player.Uid == "Unknown"))
                        {
                            player.Uid = uid;
                        }
                    }
                }

                Manager.props.adminLog = returnString;
                return returnString;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when getting admin log");
                return string.Empty;
            }
        }

        public void UpdateBeConfig(string path)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error when updating be config");
            }
        }


        #region ServerConfig
        public ServerConfig LoadServerConfig(string serverConfigPath)
        {
            if (File.Exists(serverConfigPath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(Path.Combine(Manager.SERVER_PATH, Manager.managerConfig.serverConfigName)))
                    {
                        string serverConfigText = reader.ReadToEnd();
                        _serverConfig = ServerConfigSerializer.Deserialize(serverConfigText);
                        return _serverConfig;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error when loading server config");
                }
            }

            _serverConfig = new ServerConfig();
            _serverConfig.SetDefaultValues();
            return _serverConfig;
        }

        public void AdjustServerConfig(string _missionName, int _instanceId, int _steamQueryPort)
        {
            PropertyValue? template = _serverConfig.GetPropertyValue("template");
            if (template != null)
            {
                template.Value = _missionName;
            }
            else
            {
                _serverConfig.Properties.Add(new PropertyValue(_serverConfig.GetNextID(), "template", DataType.Text, _missionName, ""));
            }

            PropertyValue? instanceId = _serverConfig.GetPropertyValue("instanceId");
            if (instanceId != null)
            {
                instanceId.Value = _instanceId;
            }
            else
            {
                _serverConfig.Properties.Add(new PropertyValue(_serverConfig.GetNextID(), "instanceId", DataType.Text, _instanceId, ""));
            }

            PropertyValue? steamQueryPort = _serverConfig.GetPropertyValue("steamQueryPort");
            if (steamQueryPort != null)
            {
                steamQueryPort.Value = _steamQueryPort;
            }
            else
            {
                _serverConfig.Properties.Add(new PropertyValue(_serverConfig.GetNextID(), "steamQueryPort", DataType.Text, _steamQueryPort, ""));
            }
        }

        public void SaveServerConfig(string serverConfigPath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(serverConfigPath))
                {
                    string serverConfigText = ServerConfigSerializer.Serialize(_serverConfig);
                    writer.Write(serverConfigText);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error saving the server config");
            }
        }
        #endregion ServerConfig

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
                Logger.Error(ex, "Error getting keys folder");
                return string.Empty;
            }
        }
    }
}