using System.Net;
using Domain.Profile;
using Domain.Scheduler;

namespace Application.IService;

public interface IPlayerService
{

    public List<User> GetPlayers();
    public User? GetPlayer(string id);
    public List<ServerPlayerInformation> GetServerPlayerInformation(int id);
    public HttpStatusCode CreateServerPlayer(string playerId, int instanceId, bool isWhitelisted, bool isBanned, string roleName);
    public List<Role> GetRoles(int instanceId);
    public List<string> GetRoleNames(int instanceId);
    public Role? GetRole(string name, int instanceId);
    public HttpStatusCode AddRole(string name, int instanceId);
    public void ReadOutRoles(int instanceId);
    public Dictionary<string, PlayerPermissions> ReadOutServerPlayerRoles(int instanceId);

    public HttpStatusCode SaveServerPlayerRole(string serverPlayerId, string playerGuid, int instanceId,
        string roleName);
}