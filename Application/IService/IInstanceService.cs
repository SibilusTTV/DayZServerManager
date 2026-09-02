using System.Net;
using Domain.Manager;
using Domain.ServerConfig;

namespace Application.IService;

public interface IInstanceService
{
    public IServerInstance CreateServer(int id);
    public IServerInstance? GetServer(int id);
    public void StartServer(int id);
    public void StopServer(int id);
    public void RemoveServer(int id);
    public IEnumerable<IServerInstance> GetAllServers();
    public ServerInformation? GetServerInformation(int id);
    public List<ServerInformation> GetServerInformations();
    public List<PropertyValue> GetServerConfig(int id);
    public HttpStatusCode SaveServerConfig(List<PropertyValue> properties, int id);
    public void Initialize();
    public void Dispose();
    public Instance? GetInstance(int id);
    public List<Instance> GetInstances();
    public Instance? CreateEmptyInstanceConfig();
    public HttpStatusCode UpdateInstanceConfig(Instance instanceConfig);
    public HttpStatusCode CreateInstance(Instance instanceConfig);
    public HttpStatusCode BanPlayer(string playerGuid, int instanceId, string reason, int duration);
    public HttpStatusCode UnbanPlayer(string playerGuid, int instanceId);
    public void KickPlayer(string playerGuid, int instanceId, string reason);
    public HttpStatusCode WhitelistPlayer(string playerGuid, int instanceId);
    public HttpStatusCode UnwhitelistPlayer(string playerGuid, int instanceId);
    public void SetMissionNeedsUpdatingForServer(int id);
}