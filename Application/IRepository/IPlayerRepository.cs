using System.Net;
using Domain.Profile;
using Domain.Scheduler;

namespace Application.IRepository;

public interface IPlayerRepository
{
    public List<User> GetAllPlayers();
    public User? GetPlayer(string id);
    public List<User> GetPlayersByName(string name);
    public void CreateEditPlayer(User user);
    public List<ServerPlayer> GetServerPlayersForInstance(string instanceId);
    public ServerPlayer? GetServerPlayer(string serverPlayerId);
    public ServerPlayer? GetServerPlayerByGuid(string playerGuid, string instanceId);
    public List<ServerPlayerInformation> GetServerPlayerInformationForInstance(string instanceId);
    public HttpStatusCode CreateEditServerPlayer(ServerPlayer player);
    public List<ServerPlayer> GetBannedServerPlayers(string instanceId);
    public List<string> GetWhitelistedPlayerNames(string instanceId);
    public void WhitelistPlayer(string serverPlayerId);
    public void UnWhitelistPlayer(string serverPlayerId);
    public List<Role> GetRoles(string instanceId);
    public List<string> GetRoleNames(string instanceId);
    public Role? GetRole(string name, string instanceId);
    public HttpStatusCode AddRole(string name, string instanceId);
    public void ReadOutRoles(string profileFolder, string instanceId);
    public Dictionary<string, PlayerPermissions> ReadOutServerPlayerRoles(string profileFolder, string instanceId);

    public HttpStatusCode SaveServerPlayerRole(string profileFolder, ServerPlayer serverPlayer,
        string roleName);
}