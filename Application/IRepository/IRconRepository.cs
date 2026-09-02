using System.Net;
using BytexDigital.BattlEye.Rcon.Domain;
using Domain.Scheduler;

namespace Application.IRepository;

public interface IRconRepository
{
    public string ChatLog { get; }
    public List<ConnectedPlayer> ConnectedPlayers { get; }
    public void InitializeRconRepository(string ip, int port, string password);
    public bool Connect();
    public void SendCommand(string command);
    public void Disconnect();
    public bool IsConnected();
    public List<ConnectedPlayer> GetPlayers();
    public void KickPlayer(int id, string reason, string name);
    public HttpStatusCode BanPlayer(string guid, string reason, int duration, string name);
    public HttpStatusCode UnbanPlayer(int banId, string name);
    public void ReloadBans();
    public List<PlayerBan> GetBans();
    public void Shutdown();
}