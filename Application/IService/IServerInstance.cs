using Domain.Manager;
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
}