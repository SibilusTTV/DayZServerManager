using DayZServerManager.ConfigClasses;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace DayZServerManager
{
    internal class Server
    {
        // Server Variables
        private Config config;
        int dayZBranch;

        // Other Variables
        private bool updatedMods;

        Process serverProcess;
        Process becProcess;
        Process becUpdateProcess;

        public Server(Config config)
        {
            this.config = config;
            int dayZBranch = 223350;
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
            if (becProcess != null)
            {
                return !becProcess.HasExited;
            }
            else
            {
                return false;
            }
        }

        public bool CheckForUpdatedMods()
        {
            if (updatedMods && !(becUpdateProcess != null && becUpdateProcess.HasExited))
            {
                updatedMods = false;
                try
                {
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

        public void KillServers()
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

        public void StartServer()
        {
            string clientModsToLoad = "";
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

            string serverModsToLoad = "";
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
                string startParameters = $"-instanceId=1 -config=serverDZ.cfg -profiles=Profiles -port=2302 {clientModsToLoad} {serverModsToLoad} -cpuCount=8 -noFilePatching -dologs -adminlog -freezecheck";
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Starting Server");
                serverProcess = Process.Start(Path.Combine(config.serverPath, "DayZServer_x64.exe"), startParameters);
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Server starting up");
            }
            catch (Exception ex)
            { 
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
}

        public void StartBEC()
        {
            try
            {
                string becStartParameters = $"-f Config.cfg --dsc";
                becProcess = new Process();
                becProcess.StartInfo.FileName = Path.Combine(config.becPath,"Bec.exe");
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

        public void UpdateServer()
        {
            try
            {
                string serverUpdateArguments = $"+force_install_dir {Path.Combine("..", config.serverPath)} +login {config.steamUsername} {config.steamPassword} +\"app_update {dayZBranch}\" +quit";
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
            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Updating Mods");

            foreach (Mod mod in config.clientMods)
            {
                if (mod != null)
                {
                    if (hasToUpdate)
                    {
                        UpdateMod(mod);
                    }
                    if (hasToMove)
                    {
                        MoveMod(mod);
                    }
                }
            }

            foreach (Mod mod in config.serverMods)
            {
                if (mod != null)
                {
                    if (hasToUpdate)
                    {
                        UpdateMod(mod);
                    }
                    if (hasToMove)
                    {
                        MoveMod(mod);
                    }
                }
            }

            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mods updated");
        }

        private void UpdateMod(Mod mod)
        {
            try
            {
                if (mod != null)
                {
                    string modUpdateArguements = $"+login {config.steamUsername} {config.steamPassword} +workshop_download_item 221100 {mod.workshopID.ToString()} +quit";
                    Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} {config.steamCMDPath} {modUpdateArguements}");
                    Process p = Process.Start(config.steamCMDPath, modUpdateArguements);
                    p.WaitForExit();
                    string modKeysPath = Path.Combine(config.workshopPath, mod.workshopID.ToString());
                    if (FileSystem.DirectoryExists(modKeysPath))
                    {
                        List<string> fileNames = FileSystem.GetFiles(modKeysPath).ToList<string>();
                        if (fileNames.Count > 0)
                        {
                            string modKeyPath = Path.Combine(modKeysPath, fileNames[0]);
                            if (File.Exists(modKeyPath))
                            {
                                DateTime changingDate = File.GetLastWriteTimeUtc(modKeyPath);
                                if (mod.lastUpdated != null)
                                {
                                    if (mod.lastUpdated != changingDate)
                                    {
                                        mod.lastUpdated = changingDate;
                                        updatedMods = true;
                                    }
                                }
                                else
                                {
                                    mod.lastUpdated = changingDate;
                                }
                            }
                        }
                        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mod {mod.workshopID.ToString()} was downloaded");
                    }
                }
            }
            catch(System.Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        private void MoveMod(Mod mod)
        {
            try
            {
                if (mod != null)
                {
                    string steamModPath = Path.Combine(config.workshopPath, mod.workshopID.ToString());
                    string serverModPath = "";
                    serverModPath = Path.Combine(config.serverPath, mod.name);

                    Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                    if (FileSystem.DirectoryExists(steamModPath))
                    {
                        if (FileSystem.DirectoryExists(serverModPath))
                        {
                            FileSystem.DeleteDirectory(serverModPath, DeleteDirectoryOption.DeleteAllContents);
                        }
                        FileSystem.CopyDirectory(steamModPath, serverModPath);

                        string serverKeysPath = Path.Combine(config.serverPath, "keys");
                        string modKeysPath = "";
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
            }
            catch (Exception ex)
            {
                Console.WriteLine( Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        public void BackupServerData()
        {
            try
            {
                if (FileSystem.DirectoryExists(config.backupPath))
                {
                    string newestBackupPath = Path.Combine(config.backupPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                    string dataPath = Path.Combine(config.serverPath, "mpmissions", "dayzOffline.chernarusplus", "storage_1");
                    string profilePath = Path.Combine(config.serverPath, "Profiles");
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }
    }
}