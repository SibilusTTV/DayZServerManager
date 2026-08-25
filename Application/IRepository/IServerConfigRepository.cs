using Domain.ServerConfig;

namespace Application.IRepository;

public interface IServerConfigRepository
{
    public ServerConfig Get(string configPath);

    public void Save(ServerConfig serverConfig, string serverConfigPath);
}