using Domain.Scheduler;

namespace Domain.Manager;

public class ServerInformation
{
    public string managerStatus { get; set; }
    public string dayzServerStatus { get; set; }
    public int playersCount { get; set; }
    public List<ConnectedPlayer> players { get; set; }
    public string chatLog { get; set; }
    public string adminLog { get; set; }

    public ServerInformation()
    {
        managerStatus = "";
        dayzServerStatus = "";
        playersCount = 0;
        players = [];
        chatLog = "";
        adminLog = "";
    }
    
    public ServerInformation(string _managerStatus, string _dayzServerStatus, int _playersCount, string _chatLog, string _adminLog)
    {
        managerStatus = _managerStatus;
        dayzServerStatus = _dayzServerStatus;
        playersCount = _playersCount;
        players = new List<ConnectedPlayer>();
        chatLog = _chatLog;
        adminLog = _adminLog;
    }
}