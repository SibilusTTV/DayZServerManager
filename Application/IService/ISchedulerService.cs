using Application.Service;
using Domain.Manager;
using Domain.Scheduler;

namespace Application.IService;

public interface ISchedulerService
{
    public IRconService? RconClient { get; }

    public void InitializeScheduler(string ip, int port, string password, int interval, bool onlyRestarts,
        List<CustomMessage> customMessages, string serverFolderName);
    public bool Connect();
    public SchedulerInformation GetSchedulerInformation();
    public List<string> GetWhitelistedPlayers(string serverFolderName);
    public void SaveWhitelistedPlayers(string serverFolderName, List<string> whitelistedPlayers);
    public void Disconnect();
    public void ChangeToNormalMode();
    public void ChangeToUpdateMode();
    public void KillAutomaticTasks();
    public void KillCustomTasks();
    public bool IsConnected();
    public void GetPlayers();
    public void KickPlayer(string guid, string reason, string name);
    public void BanPlayer(Guid serverPlayerId, string reason, int duration);
    public void UnbanPlayer(Guid serverPlayerId);
    public void WhitelistPlayer(Guid serverPlayerId, string name, string serverFolderName);
    public void UnwhitelistPlayer(Guid serverPlayerId, string name, string serverFolderName);
    public void SendCommand(string command);
    public void Shutdown();
    public void LoadBans();
}