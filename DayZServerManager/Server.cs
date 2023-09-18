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
        private int steamCMDDelay;

        // Other Variables
        private bool updatedMods;
        private Dictionary<int, string> clientMods;
        private Dictionary<int, string> serverMods;
        private Dictionary<int, DateTime> modUpdateTimes;

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

            clientMods = new Dictionary<int, string>();
            serverMods = new Dictionary<int, string>();
            modUpdateTimes = new Dictionary<int, DateTime>();

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
                                clientMods.Add(Int32.Parse(l[0].Trim()), l[1].Trim());
                            }
                            else if (currentModsAdded == "Server Mods")
                            {
                                string[] l = line.Trim().Split(',');
                                serverMods.Add(Int32.Parse(l[0].Trim()), l[1].Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    becUpdateProcess = Process.Start(Path.Combine(becPath, "BEC.exe"), becStartParameters);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            return false;
        }

        public void KillServers()
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
                Console.WriteLine("\n Starting Server");
                serverProcess = Process.Start(Path.Combine(dayZServerPath, "DayZServer_x64.exe"), startParameters);
                Console.WriteLine("\n Server starting up");
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString());
            }
}

        public void StartBEC()
        {
            try
            {
                string becStartParameters = $"-f Config.cfg --dsc";
                becProcess = new Process();
                becProcess.StartInfo.FileName = Path.Combine(becPath, "BEC.exe");
                becProcess.StartInfo.Arguments = becStartParameters;
                becProcess.StartInfo.UseShellExecute = false;
                becProcess.StartInfo.CreateNoWindow = true;
                becProcess.StartInfo.RedirectStandardInput = true;
                becProcess.StartInfo.RedirectStandardOutput = false;
                Console.WriteLine("\n Starting BEC");
                becProcess.Start();
                Console.WriteLine("\n BEC started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void UpdateAndMoveMods(bool hasToUpdate, bool hasToMove)
        {
            Console.WriteLine("Updating Mods");

            foreach (int key in clientMods.Keys)
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

            foreach (int key in serverMods.Keys)
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

            Console.WriteLine("Mods updated");
        }

        public void UpdateServer()
        {
            try
            {
                string serverUpdateArguments = $"+force_install_dir {dayZServerPath} +login {steamLogin} +\"app_update {dayZBranch}\" +quit";
                Console.WriteLine("\n Updating the DayZ Server");
                Process p = Process.Start(steamCMDPath, serverUpdateArguments);
                p.WaitForExit();
                Console.WriteLine("\n DayZ Server updated");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void UpdateMod(int key)
        {
            try
            {
                string modUpdateArguements = $"+login {steamLogin} +workshop_download_item 221100 {key.ToString()} +quit";
                Console.WriteLine($"\n{steamCMDPath} {modUpdateArguements}");
                Process p = Process.Start(steamCMDPath, modUpdateArguements);
                p.WaitForExit();
                Console.WriteLine($"\n Mod {key.ToString()} was downloaded");
                string modKeysPath = Path.Combine(steamCMDWorkshopPath, key.ToString(), "Keys");
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
            }
            catch(System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void MoveMod(int key)
        {
            string steamModPath = Path.Combine(steamCMDWorkshopPath, key.ToString());
            string serverModPath = Path.Combine(dayZServerPath, clientMods[key]);
            Console.WriteLine($"\n Moving the mod from {steamModPath} to the DayZ Server Path under {serverModPath}");
            try
            {
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
                    Console.WriteLine("\n Mod was moved");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}