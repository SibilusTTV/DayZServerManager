// See https://aka.ms/new-console-template for more information
using DayZServerManager;
using System.Diagnostics;
using System.Runtime.CompilerServices;

string steamLogin = "";
string serverPath = "";
string steamCMD = "";
string becPath = "";
string modlistPath = "";
string workshopPath = "";
try
{
    using (var reader = new StreamReader("GlobalVariables.txt"))
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("steamLogin"))
            {
                steamLogin = line.Substring(line.IndexOf('=') + 1).Trim();
            }
            else if (line.StartsWith("serverPath"))
            {
                serverPath = line.Substring(line.IndexOf('=') + 1).Trim();
            }
            else if (line.StartsWith("steamCMD"))
            {
                steamCMD = line.Substring(line.IndexOf('=') + 1).Trim();
            }
            else if (line.StartsWith("becPath"))
            {
                becPath = line.Substring(line.IndexOf('=') + 1).Trim();
            }
            else if (line.StartsWith("modlistPath"))
            {
                modlistPath = line.Substring(line.IndexOf('=') + 1).Trim();
            }
            else if (line.StartsWith("workshopPath"))
            {
                workshopPath = line.Substring(line.IndexOf('=') + 1).Trim();
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

if (string.IsNullOrEmpty(steamLogin))
{
    Console.WriteLine("Enter steam username and password seperated by a space");
    steamLogin = Console.ReadLine();
}
if (string.IsNullOrEmpty(serverPath))
{
    serverPath = "Server";
}
if (string.IsNullOrEmpty(steamCMD))
{
    steamCMD = "SteamCMD\\steamcmd.exe";
}
if (string.IsNullOrEmpty(becPath))
{
    becPath = "BEC";
}
if (string.IsNullOrEmpty(modlistPath))
{
    modlistPath = "modlist.txt";
}
if (string.IsNullOrEmpty(workshopPath))
{
    workshopPath = "SteamCMD\\steamapps\\workshop\\content\\221100";
}

Server s = new Server(steamLogin, serverPath, steamCMD, becPath, modlistPath, workshopPath);

AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

s.UpdateServer();
s.UpdateAndMoveMods(true, true);
s.StartServer();
s.StartBEC();
Thread.Sleep(30000);

int i = 1;
bool kill = false;
while (!kill)
{
    if (!s.CheckServer())
    {
        s.UpdateServer();
        s.UpdateAndMoveMods(false, true);
        s.StartServer();
    }
    else
    {
        Console.WriteLine("\n The Server is still running");
    }
    if (!s.CheckBEC())
    {
        s.StartBEC();
    }
    else
    {
        Console.WriteLine("\n BEC is still running");
    }
    if (i%40 == 0)
    {
        s.UpdateAndMoveMods(true, false);
        s.CheckForUpdatedMods();
    }
    i++;
    Thread.Sleep(30000);
}
s.KillServers();

void OnProcessExit(object sender, EventArgs e)
{
    s.KillServers();
}