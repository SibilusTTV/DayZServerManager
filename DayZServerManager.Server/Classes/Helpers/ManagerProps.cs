namespace DayZServerManager.Server.Classes.Helpers
{
    public class ManagerProps
    {
        public string _serverStatus;
        public string _updateStatus;
        public ManagerProps(string serverStatus, string updateStatus) 
        {
            _serverStatus = serverStatus;
            _updateStatus = updateStatus;
        }
    }
}
