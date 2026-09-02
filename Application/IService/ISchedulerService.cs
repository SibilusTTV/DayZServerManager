using System.Net;
using Application.Service;
using Domain.Manager;
using Domain.Scheduler;

namespace Application.IService;

public interface ISchedulerService
{
    public void InitializeScheduler(int instanceId, string ip, int port, string password, int interval,
        bool onlyRestarts, List<CustomMessage> customMessages, string serverFolderName);
    public bool Connect();
    public SchedulerInformation GetSchedulerInformation();
    public List<string> GetWhitelistedPlayers(string serverFolderName);
    public HttpStatusCode SaveWhitelistedPlayers(string serverFolderName, List<string> whitelistedPlayers);
    public void Disconnect();
    public void ChangeToNormalMode();
    public void ChangeToUpdateMode();
    public void KillAutomaticTasks();
    public void KillCustomTasks();
    public bool IsConnected();
    public int UpdatePlayers(int instanceId);
    public void KickPlayer(string guid, int instanceId, string reason);
    public HttpStatusCode BanPlayer(string playerGuid, int instanceId, string reason, int duration);
    public HttpStatusCode UnbanPlayer(string playerGuid, int instanceId);
    public HttpStatusCode WhitelistPlayer(string playerGuid, int instanceId);
    public HttpStatusCode UnwhitelistPlayer(string playerGuid, int instanceId);
    public void SendCommand(string command);
    public void Shutdown();
}