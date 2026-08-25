using Domain.ServerConfig;

namespace Application.IService;

public interface IServerConfigService
{
    public void UpdateServerConfig(ServerConfig serverConfig, string missionNameValue, string hostNameValue,
        int instanceIdValue, int steamPortValue, int steamQueryPortValue);
    public ServerConfig Get(string configPath);
    public void Save(ServerConfig serverConfig, string configPath);
}