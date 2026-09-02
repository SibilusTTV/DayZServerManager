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
    public List<ServerPlayer> GetServerPlayersForInstance(int instanceId);
    public ServerPlayer? GetServerPlayer(string serverPlayerId);
    public ServerPlayer? GetServerPlayerByGuid(string playerGuid, int instanceId);
    public List<ServerPlayerInformation> GetServerPlayerInformationForInstance(int instanceId);
    public HttpStatusCode CreateEditServerPlayer(ServerPlayer player);
    public List<ServerPlayer> GetBannedServerPlayers(int instanceId);
    public List<string> GetWhitelistedPlayerNames(int instanceId);
    public void WhitelistPlayer(string serverPlayerId);
    public void UnWhitelistPlayer(string serverPlayerId);
    public List<Role> GetRoles(int instanceId);
    public List<string> GetRoleNames(int instanceId);
    public Role? GetRole(string name, int instanceId);
    public HttpStatusCode AddRole(string name, int instanceId);
    public void ReadOutRoles(string profileFolder, int instanceId);
    public Dictionary<string, PlayerPermissions> ReadOutServerPlayerRoles(string profileFolder, int instanceId);

    public HttpStatusCode SaveServerPlayerRole(string profileFolder, ServerPlayer serverPlayer,
        string roleName);
}