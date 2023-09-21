// See https://aka.ms/new-console-template for more information
using DayZServerManager;
using DayZServerManager.ConfigClasses;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

Config config = null;

try
{
    using (StreamReader reader = new StreamReader("Config.json"))
    {
        string json = reader.ReadToEnd();
        config = JsonSerializer.Deserialize<Config>(json);
        reader.Close();
    }
}
catch (Exception ex)
{
    Console.WriteLine( DateTime.Now.ToString() + ex.ToString());
}

if (config != null)
{

    if (string.IsNullOrEmpty(config.steamUsername))
    {
        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Enter steam username");
        config.steamUsername = Console.ReadLine();
    }

    if (string.IsNullOrEmpty(config.steamPassword))
    {
        Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} Enter steam password");
        config.steamPassword = Console.ReadLine();
    }

    Server s = new Server(config);

    AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

    s.BackupServerData();
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
            s.BackupServerData();
            s.UpdateServer();
            s.UpdateAndMoveMods(false, true);
            s.StartServer();
        }
        else
        {
            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} The Server is still running");
        }
        if (!s.CheckBEC())
        {
            s.StartBEC();
        }
        else
        {
            Console.WriteLine($"{Environment.NewLine} {DateTime.Now.ToString("[HH:mm:ss]")} BEC is still running");
        }
        if (i % 40 == 0)
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
}
