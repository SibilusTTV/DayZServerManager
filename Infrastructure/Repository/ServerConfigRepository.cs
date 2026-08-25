using Application.IRepository;
using Domain.ServerConfig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class ServerConfigRepository : IServerConfigRepository
{
    private readonly ILogger<ServerConfigRepository> _logger;
    private readonly IServerConfigSerializerRepository _serverConfigSerializerRepository;

    public ServerConfigRepository(ILogger<ServerConfigRepository> logger, IServerConfigSerializerRepository serverConfigSerializerRepository)
    {
        _logger = logger;
        _serverConfigSerializerRepository = serverConfigSerializerRepository;
    }
    
    public ServerConfig Get(string configPath)
    {
        if (File.Exists(configPath))
        {
            try
            {
                using var reader = new StreamReader(configPath);
                
                var serverConfigText = reader.ReadToEnd();
                return _serverConfigSerializerRepository.Deserialize(serverConfigText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when loading server config");
            }
        }

        var serverConfig = new ServerConfig();
        serverConfig.SetDefaultValues();
        return serverConfig;
    }

    public void Save(ServerConfig serverConfig, string serverConfigPath)
    {
        try
        {
            using var writer = new StreamWriter(serverConfigPath);
            
            var serverConfigText = _serverConfigSerializerRepository.Serialize(serverConfig);
            writer.Write(serverConfigText);
            writer.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving the server config");
        }
    }
}