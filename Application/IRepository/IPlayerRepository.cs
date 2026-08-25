using System.Net;
using Domain.Scheduler;

namespace Application.IRepository;

public interface IPlayerRepository
{
    public List<Player> GetAllPlayers();
    public Player? GetPlayer(string id);
    public List<Player> GetPlayersByName(string name);
    public void CreateEditPlayer(Player player);
    public List<ServerPlayer> GetServerPlayersForInstance(string instanceId);
    public ServerPlayer? GetServerPlayer(string playerId);
    public List<ServerPlayerInformation> GetServerPlayerInformationForInstance(string instanceId);
    public HttpStatusCode CreateEditServerPlayer(ServerPlayer player);
    public void ClearBans();
    public ServerPlayer? GetBannedServerPlayer(int banId, string id, string reason);
    public void CreateNewBan(int banId, string id, int remainingTime, string reason);
    public HttpStatusCode RemoveBan(string id);
    public void UpdateRemainingTime(string id, int remainingTime);
    public List<string> GetWhitelistedPlayerNames(string instanceId);
    public void WhitelistPlayer(string serverPlayerId);
    public void UnWhitelistPlayer(string serverPlayerId);
}