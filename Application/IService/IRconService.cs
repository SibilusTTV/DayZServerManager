
using Domain.Scheduler;

namespace Application.IService;

public interface IRconService
{
    
    public string ChatLog { get; }
    public int PlayersCount { get; }
    public List<ConnectedPlayer> ConnectedPlayers { get; }

    public void InitializeRconService(string ip, int port, string password, SchedulerConfig Config);

    public bool Connect();
    public void SendCommand(string command);
    public void Disconnect();
    public bool IsConnected();
    public void GetPlayers();
    public void KickPlayer(int id, string reason, string name);
    public void BanPlayer(Guid guid, string reason, int duration, string name);
    public void UnbanPlayer(int banId, string name);
    public void ReloadBans();
    public void GetBans();
    public void Shutdown();

}