namespace DayZServerManager.Server.Classes.Helpers
{
    public class ManagerProps
    {
        public string managerStatus {  get; set; }
        public string dayzServerStatus { get; set; }
        public string steamCMDStatus { get; set; }
        public int playersCount { get; set; }
        public List<string> players {  get; set; }
        public ManagerProps(string _managerStatus, string _dayzServerStatus, string _steamCMDStatus, int _playersCount) 
        {
            managerStatus = _managerStatus;
            steamCMDStatus = _dayzServerStatus;
            dayzServerStatus = _dayzServerStatus;
            playersCount = _playersCount;
            players = new List<string>();
        }
    }
}
