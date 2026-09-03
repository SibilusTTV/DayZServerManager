using System.Net;
using Domain.Manager;
using Domain.Scheduler;
using Domain.ServerConfig;

namespace Application.IService;

public interface IServerInstance
{
    bool IsRunning { get; }
    bool MissionNeedsUpdating { get; set; }
    public ServerConfig ServerConfig { get; set; }
    public void StartTimer(string steamUsername, string steamPassword);
    public void Stop();
    void Dispose();
    public ServerInformation GetServerInformation();
    public void KillServerProcesses();
    public HttpStatusCode BanPlayer(string playerGuid, int instanceId, string reason, int duration);
    public HttpStatusCode UnbanPlayer(string playerGuid, int instanceId);
    public void KickPlayer(string playerGuid, int instanceId, string reason);
    public HttpStatusCode WhitelistPlayer(string playerGuid, int instanceId);
    public HttpStatusCode UnwhitelistPlayer(string playerGuid, int instanceId);
    public SchedulerConfig? GetSchedulerConfig();
    public void CreateEditSchedulerConfig(SchedulerConfig schedulerConfig);
}