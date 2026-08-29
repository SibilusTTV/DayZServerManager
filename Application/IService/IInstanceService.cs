using System.Net;
using Domain.Manager;
using Domain.ServerConfig;

namespace Application.IService;

public interface IInstanceService
{
    public IServerInstance CreateServer(Instance instance);
    public IServerInstance? GetServer(string id);
    public void StartServer(string id);
    public void StopServer(string id);
    public void RemoveServer(string id);
    public IEnumerable<IServerInstance> GetAllServers();
    public ServerInformation? GetServerInformation(string id);
    public List<ServerInformation> GetServerInformations();
    public List<PropertyValue> GetServerConfig(string id);
    public HttpStatusCode SaveServerConfig(List<PropertyValue> properties, string id);
    public void Initialize();
    public void Dispose();
    public Instance? GetInstance(string id);
    public List<Instance> GetInstances();
    public Instance? CreateEmptyInstanceConfig();
    public HttpStatusCode UpdateInstanceConfig(Instance instanceConfig);
    public HttpStatusCode CreateInstance(Instance instanceConfig);
    public HttpStatusCode BanPlayer(string playerGuid, string instanceId, string reason, int duration);
    public HttpStatusCode UnbanPlayer(string playerGuid, string instanceId);
    public void KickPlayer(string playerGuid, string instanceId, string reason);
    public HttpStatusCode WhitelistPlayer(string playerGuid, string instanceId);
    public HttpStatusCode UnwhitelistPlayer(string playerGuid, string instanceId);
    public void SetMissionNeedsUpdatingForServer(string id);
}