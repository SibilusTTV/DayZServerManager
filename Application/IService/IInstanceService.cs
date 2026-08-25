using System.Net;
using Domain.Manager;
using Domain.ServerConfig;

namespace Application.IService;

public interface IInstanceService
{
    public IServerInstance CreateServer(Instance instance);
    public IServerInstance? GetServer(Guid id);
    public void StartServer(Guid id);
    public void StopServer(Guid id);
    public void RemoveServer(Guid id);
    public IEnumerable<IServerInstance> GetAllServers();
    public ServerInformation? GetServerInformation(Guid id);
    public List<ServerInformation> GetServerInformations();
    public ServerConfig GetServerConfig(Guid id);
    public HttpStatusCode SaveServerConfig(ServerConfig serverConfig, Guid id);
    public void Initialize();
    public void Dispose();
    public Instance? GetInstance(Guid id);
    public List<Instance> GetInstances();
    public Instance? CreateEmptyInstanceConfig();
    public HttpStatusCode UpdateInstanceConfig(Instance instanceConfig);
    public HttpStatusCode CreateInstance(Instance instanceConfig);
}