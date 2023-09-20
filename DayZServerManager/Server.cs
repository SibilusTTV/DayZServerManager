using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace DayZServerManager
{
    internal class Server
    {
        // Server Variables
        private string steamLogin;
        private int dayZBranch;
        private string dayZServerPath;
        private string steamCMDPath;
        private string becPath;
        private string dayZModList;
        private string steamCMDWorkshopPath;

        // Other Variables
        private bool updatedMods;
        private Dictionary<long, string> clientMods;
        private Dictionary<long, string> serverMods;
        private Dictionary<long, DateTime> modUpdateTimes;

        Process serverProcess;
        Process becProcess;
        Process becUpdateProcess;

        public Server(string login, string serverPath, string steamPath, string becPath, string modlist, string workshopPath)
        {

            steamLogin = login;
            dayZBranch = 223350;
            dayZServerPath = serverPath;
            steamCMDPath = steamPath;
            this.becPath = becPath;
            dayZModList = modlist;
            steamCMDWorkshopPath = workshopPath;

            clientMods = new Dictionary<long, string>();
            serverMods = new Dictionary<long, string>();
            modUpdateTimes = new Dictionary<long, DateTime>();

            string currentModsAdded = "";
            try
            {
                using (var reader = new StreamReader(dayZModList))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("Client Mods"))
                        {
                            currentModsAdded = "Client Mods";
                        }
                        else if (line.Contains("Server Mods"))
                        {
                            currentModsAdded = "Server Mods";
                        }
                        else
                        {
                            if (currentModsAdded == "Client Mods")
                            {
                                string[] l = line.Trim().Split(',');
                                clientMods.Add(Int64.Parse(l[0].Trim()), l[1].Trim());
                            }
                            else if (currentModsAdded == "Server Mods")
                            {
                                string[] l = line.Trim().Split(',');
                                serverMods.Add(Int64.Parse(l[0].Trim()), l[1].Trim());
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
                    becUpdateProcess = Process.Start(Path.Combine(becPath, "Bec.exe"), becStartParameters);
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
            foreach (string clientMod in clientMods.Values)
            {
                clientModsToLoad += clientMod + ";";
            }
            if (!string.IsNullOrEmpty(clientModsToLoad))
            {
                clientModsToLoad = clientModsToLoad.Remove(clientModsToLoad.Length - 1);
            }

            string serverModsToLoad = "";
            foreach (string serverMod in serverMods.Values)
            {
                serverModsToLoad += serverMod + ";";
            }
            if (!string.IsNullOrEmpty(serverModsToLoad))
            {
                serverModsToLoad = serverModsToLoad.Remove(serverModsToLoad.Length - 1);
            }

            try
            {
                string startParameters = $"-instanceId=1 -config=serverDZ.cfg -profiles=Profiles -port=2302 \"-mod={clientModsToLoad}\" \"-serverMod={serverModsToLoad}\" -cpuCount=8 -noFilePatching -dologs -adminlog -freezecheck";
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Starting Server");
                serverProcess = Process.Start(Path.Combine(dayZServerPath, "DayZServer_x64.exe"), startParameters);
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
                becProcess.StartInfo.FileName = Path.Combine(becPath,"Bec.exe");
                becProcess.StartInfo.WorkingDirectory = becPath;
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
                string serverUpdateArguments = $"+force_install_dir {Path.Combine("..", dayZServerPath)} +login {steamLogin} +\"app_update {dayZBranch}\" +quit";
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Updating the DayZ Server");
                Process p = Process.Start(steamCMDPath, serverUpdateArguments);
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

            foreach (long key in clientMods.Keys)
            {
                if (hasToUpdate)
                {
                    UpdateMod(key);
                }
                if (hasToMove)
                {
                    MoveMod(key);
                }
            }

            foreach (long key in serverMods.Keys)
            {
                if (hasToUpdate)
                {
                    UpdateMod(key);
                }
                if (hasToMove)
                {
                    MoveMod(key);
                }
            }

            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mods updated");
        }

        private void UpdateMod(long key)
        {
            try
            {
                string modUpdateArguements = $"+login {steamLogin} +workshop_download_item 221100 {key.ToString()} +quit";
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} {steamCMDPath} {modUpdateArguements}");
                Process p = Process.Start(steamCMDPath, modUpdateArguements);
                p.WaitForExit();
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mod {key.ToString()} was downloaded");
            }
            catch(System.Exception ex)
            {
                Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }

        private void MoveMod(long key)
        {
            try
            {
                string modKeysPath = "";
                string modKeysPath1 = Path.Combine(steamCMDWorkshopPath, key.ToString(), "Keys");
                string modKeysPath2 = Path.Combine(steamCMDWorkshopPath, key.ToString(), "Key");

                if (FileSystem.DirectoryExists(modKeysPath1))
                {
                    modKeysPath = modKeysPath1;
                }
                else if (FileSystem.DirectoryExists(modKeysPath2))
                {
                    modKeysPath = modKeysPath2;
                }

                if (FileSystem.DirectoryExists(modKeysPath))
                {
                    List<string> fileNames = FileSystem.GetFiles(modKeysPath).ToList<string>();
                    if (fileNames.Count > 0)
                    {
                        string modKeyPath = Path.Combine(modKeysPath, fileNames[0]);
                        if (File.Exists(modKeyPath))
                        {
                            DateTime changingDate = File.GetLastWriteTimeUtc(modKeyPath);
                            if (modUpdateTimes.ContainsKey(key))
                            {
                                if (modUpdateTimes[key] != changingDate)
                                {
                                    modUpdateTimes[key] = changingDate;
                                    updatedMods = true;
                                }
                            }
                            else
                            {
                                modUpdateTimes.Add(key, changingDate);
                            }
                        }
                    }
                }

                string steamModPath = Path.Combine(steamCMDWorkshopPath, key.ToString());
                string serverModPath = Path.Combine(dayZServerPath, clientMods[key]);
                Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
                if (FileSystem.DirectoryExists(steamModPath))
                {
                    if (FileSystem.DirectoryExists(serverModPath))
                    {
                        FileSystem.DeleteDirectory(serverModPath, DeleteDirectoryOption.DeleteAllContents);
                    }
                    FileSystem.CopyDirectory(steamModPath, serverModPath);

                    string modKeyPath = Path.Combine(steamModPath, "keys");
                    string serverKeyPath = Path.Combine(dayZServerPath, "keys");

                    FileSystem.CopyDirectory(modKeyPath, serverKeyPath, true);
                    Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Mod was moved to {clientMods[key]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine( Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + ex.ToString());
            }
        }
    }
}