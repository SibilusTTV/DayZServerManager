// See https://aka.ms/new-console-template for more information
using DayZServerManager;
using System.Diagnostics;
using System.Runtime.CompilerServices;

Console.WriteLine("Enter login credentials");
string steamLogin = Console.ReadLine();
string serverPath = "\\Server";
string steamCMD = "\\SteamCMD\\steamcmd.exe";
string becPath = "\\BEC";
string modlistPath = "\\modlist.txt";
string workshopPath = "\\SteamCMD\\steamapps\\workshop\\content\\221100";

Server s = new Server(steamLogin, serverPath, steamCMD, becPath, modlistPath, workshopPath);

AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

s.UpdateServer();
s.UpdateAndMoveMods(true, true);
s.StartServer();
s.StartBEC();

int i = 1;
bool kill = false;
while (!kill)
{
    if (!s.CheckServer())
    {
        s.UpdateAndMoveMods(false, true);
        s.UpdateServer();
        s.StartServer();
    }
    if (!s.CheckBEC())
    {
        s.StartBEC();
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