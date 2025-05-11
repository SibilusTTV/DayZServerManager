using DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses;

namespace DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses
{
    public class ManagerProps
    {
        public string managerStatus { get; set; }
        public string dayzServerStatus { get; set; }
        public string steamCMDStatus { get; set; }
        public int playersCount { get; set; }
        public List<ConnectedPlayer> players { get; set; }
        public string chatLog { get; set; }
        public string adminLog { get; set; }
        public ManagerProps(string _managerStatus, string _dayzServerStatus, string _steamCMDStatus, int _playersCount, string _chatLog, string _adminLog)
        {
            managerStatus = _managerStatus;
            steamCMDStatus = _dayzServerStatus;
            dayzServerStatus = _dayzServerStatus;
            playersCount = _playersCount;
            players = new List<ConnectedPlayer>();
            chatLog = _chatLog;
            adminLog = _adminLog;
        }
    }
}
