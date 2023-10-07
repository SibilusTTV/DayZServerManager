using DayZServerManager.Helpers;
using DayZServerManager.ManagerConfigClasses;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace DayZServerManager
{
    internal class Server
    {
        // Server Variables
        private Config config;
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

        public Server(Config config)
        {
            this.config = config;
            updatedModsIDs = new List<long>();
            dayZServerBranch = 223350;
            dayZGameBranch = 221100;
            serverProcess = null;
            becProcess = null;
            becUpdateProcess = null;
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
            else
            {
                return false;
            }
        }

        public void StartServer()
        {
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
                ProcessStartInfo procInf= new ProcessStartInfo();
                string startParameters = GetServerStartParameters(clientModsToLoad, serverModsToLoad);
                procInf.WorkingDirectory = config.serverPath;
                procInf.Arguments = startParameters;
                procInf.FileName = Path.Combine(config.serverPath, "DayZServer_x64.exe");
                serverProcess.StartInfo = procInf;
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Starting Server");
                serverProcess.Start();
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Server starting at {Path.Combine(config.serverPath, "DayZServer_x64.exe")} with the parameters {startParameters}");
            }
            catch (Exception ex)
            { 
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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

        public void StartBEC()
        {
            try
            {
                string becStartParameters = $"-f Config.cfg --dsc";
                becProcess = new Process();
                becProcess.StartInfo.FileName = Path.Combine(config.becPath, "Bec.exe");
                becProcess.StartInfo.WorkingDirectory = config.becPath;
                becProcess.StartInfo.Arguments = becStartParameters;
                becProcess.StartInfo.UseShellExecute = false;
                becProcess.StartInfo.CreateNoWindow = true;
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Starting BEC");
                becProcess.Start();
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} BEC started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public void UpdateServer()
        {
            try
            {
                string serverUpdateArguments = $"+force_install_dir {Path.Combine("..", config.serverPath)} +login {config.steamUsername} {config.steamPassword} +\"app_update {dayZServerBranch}\" +quit";
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Updating the DayZ Server");
                Process p = Process.Start(config.steamCMDPath, serverUpdateArguments);
                p.WaitForExit();
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} DayZ Server updated");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public void UpdateAndMoveMods(bool hasToUpdate, bool hasToMove)
        {
            List<Mod> mods = new List<Mod>();
            mods.AddRange(config.clientMods);
            mods.AddRange(config.serverMods);
            if ( hasToUpdate )
            {
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Updating Mods");
                UpdateMods(mods);
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mods updated");
            }
            if ( hasToMove )
            {
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Moving Mods");
                MoveMods(mods);
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mods moved");
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
                    Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} {config.steamCMDPath} {arguments}");
                    Process p = Process.Start(config.steamCMDPath, arguments);
                    p.WaitForExit();
                    Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} All mods were downloaded");
                    foreach (Mod mod in mods)
                    {
                        string serverModKeysPath = string.Empty;
                        if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, mod.name, "Keys")))
                        {
                            serverModKeysPath = Path.Combine(config.serverPath, mod.name, "Keys");
                        }
                        else if (FileSystem.DirectoryExists(Path.Combine(config.serverPath, mod.name, "Key")))
                        {
                            serverModKeysPath = Path.Combine(config.serverPath, mod.name, "Key");
                        }

                        string steamModKeysPath = string.Empty;
                        if (FileSystem.DirectoryExists(Path.Combine(config.workshopPath, mod.workshopID.ToString(), "Keys")))
                        {
                            steamModKeysPath = Path.Combine(config.workshopPath, mod.workshopID.ToString(), "Keys");
                        }
                        else if (FileSystem.DirectoryExists(Path.Combine(config.workshopPath, mod.workshopID.ToString(), "Key")))
                        {
                            steamModKeysPath = Path.Combine(config.workshopPath, mod.workshopID.ToString(), "Key");
                        }

                        if (steamModKeysPath != string.Empty)
                        {
                            List<string> steamModKeysFileNames = FileSystem.GetFiles(steamModKeysPath).ToList<string>();
                            if (steamModKeysFileNames.Count > 0)
                            {
                                if (serverModKeysPath != string.Empty)
                                {
                                    List<string> serverModKeysFileNames = FileSystem.GetFiles(steamModKeysPath).ToList<string>();
                                    if (steamModKeysFileNames.Count > 0)
                                    {
                                        DateTime serverModChangingDate = File.GetLastWriteTimeUtc(Path.Combine(serverModKeysPath, serverModKeysFileNames[0]));
                                        DateTime steamModChangingDate = File.GetLastWriteTimeUtc(Path.Combine(steamModKeysPath, steamModKeysFileNames[0]));
                                        if (serverModChangingDate != steamModChangingDate)
                                        {
                                            updatedModsIDs.Add(mod.workshopID);
                                            updatedMods = true;
                                        }
                                    }
                                    else
                                    {
                                        updatedModsIDs.Add(mod.workshopID);
                                        updatedMods = true;
                                    }
                                }
                                else
                                {
                                    updatedModsIDs.Add(mod.workshopID);
                                    updatedMods = true;
                                }
                            }
                        }
                    }

                    foreach (long key in updatedModsIDs)
                    {
                        Mod? mod = SearchForMod(key, mods);
                        if (mod != null)
                        {
                            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} {mod.name} was updated");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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
                        string serverModPath = string.Empty;
                        serverModPath = Path.Combine(config.serverPath, mod.name);

                        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                        if (FileSystem.DirectoryExists(steamModPath))
                        {
                            FileSystem.CopyDirectory(steamModPath, serverModPath, true);

                            string serverKeysPath = Path.Combine(config.serverPath, "keys");
                            string modKeysPath = string.Empty;
                            if (FileSystem.DirectoryExists(Path.Combine(serverModPath, "Keys")))
                            {
                                modKeysPath = Path.Combine(serverModPath, "Keys");
                            }
                            else if (FileSystem.DirectoryExists(Path.Combine(serverModPath, "Key")))
                            {
                                modKeysPath = Path.Combine(serverModPath, "Key");
                            }

                            if (FileSystem.DirectoryExists(modKeysPath))
                            {
                                FileSystem.CopyDirectory(modKeysPath, serverKeysPath, true);
                            }
                            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mod was moved to {mod.name}");
                        }
                    }

                    if (key == 2116157322 || key == 2572331007 || updatedServer)
                    {
                        updatedServer = false;
                        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Updating Mission folder");
                        MissionUpdater upd = new MissionUpdater(config);
                        upd.Update();
                        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Finished updating Mission folder");
                    }
                }

                updatedModsIDs = new List<long>();

            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public bool CheckForUpdatedMods()
        {
            if (config.RestartOnUpdate && ((updatedMods && updatedModsIDs != null && updatedModsIDs.Count > 0) || updatedServer) && !(becUpdateProcess != null && becUpdateProcess.HasExited))
            {
                updatedMods = false;
                restartingForUpdates = true;
                updatedServer = false;
                try
                {
                    if (becProcess != null && !becProcess.HasExited)
                    {
                        becProcess.Kill();
                    }
                    string becStartParameters = $"-f ConfigUpdate.cfg --dsc";
                    becUpdateProcess = Process.Start(Path.Combine(config.becPath, "Bec.exe"), becStartParameters);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
                    return false;
                }
            }
            return false;
        }

        public void BackupServerData()
        {
            try
            {
                if (FileSystem.DirectoryExists(config.backupPath))
                {
                    Console.WriteLine($"{Environment.NewLine} Backing up the server data and moving all the logs!");
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
                    Console.WriteLine($"{Environment.NewLine} Server backup and moving of the logs done");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
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
    }
}