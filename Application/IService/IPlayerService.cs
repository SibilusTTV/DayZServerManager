using System.Net;
using Domain.Scheduler;

namespace Application.IService;

public interface IPlayerService
{

    public List<Player> GetPlayers();
    public Player? GetPlayer(string id);
    public List<ServerPlayerInformation> GetServerPlayerInformation(string id);
    public HttpStatusCode CreateServerPlayer(string playerId, string instanceId, bool isWhitelisted, bool isBanned);
}