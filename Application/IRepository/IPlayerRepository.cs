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
    public ServerPlayer? GetServerPlayer(string serverPlayerId);
    public ServerPlayer? GetServerPlayerByGuid(string playerGuid, string instanceId);
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
    public List<Role> GetRoles(string instanceId);
    public List<string> GetRoleNames(string instanceId);
    public Role? GetRole(string name, string instanceId);
    public HttpStatusCode AddRole(string name, string instanceId);
    public void ReadOutRoles(string profileFolder, string instanceId);
    public void ReadOutServerPlayerRoles(string profileFolder, string instanceId);

    public HttpStatusCode SaveServerPlayerRole(string profileFolder, ServerPlayer serverPlayer,
        string roleName);
}