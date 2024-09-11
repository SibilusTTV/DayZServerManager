namespace DayZServerManager.Server.Classes.Helpers
{
    public class ManagerProps
    {
        public string managerStatus {  get; set; }
        public string dayzServerStatus { get; set; }
        public string steamCMDStatus { get; set; }
        public int players { get; set; }
        public ManagerProps(string _managerStatus, string _dayzServerStatus, string _steamCMDStatus, int _players) 
        {
            managerStatus = _managerStatus;
            steamCMDStatus = _dayzServerStatus;
            dayzServerStatus = _dayzServerStatus;
            players = _players;
        }
    }
}
