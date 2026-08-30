using System.Net;
using Domain.Profile;
using Domain.Scheduler;

namespace Application.IService;

public interface IPlayerService
{

    public List<User> GetPlayers();
    public User? GetPlayer(string id);
    public List<ServerPlayerInformation> GetServerPlayerInformation(string id);
    public HttpStatusCode CreateServerPlayer(string playerId, string instanceId, bool isWhitelisted, bool isBanned, string roleName);
    public List<Role> GetRoles(string instanceId);
    public List<string> GetRoleNames(string instanceId);
    public Role? GetRole(string name, string instanceId);
    public HttpStatusCode AddRole(string name, string instanceId);
    public void ReadOutRoles(string instanceId);
    public Dictionary<string, PlayerPermissions> ReadOutServerPlayerRoles(string instanceId);

    public HttpStatusCode SaveServerPlayerRole(string serverPlayerId, string playerGuid, string instanceId,
        string roleName);
}