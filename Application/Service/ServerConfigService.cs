using Application.IRepository;
using Application.IService;
using Domain.ServerConfig;

namespace Application.Service;

public class ServerConfigService : IServerConfigService
{
    private readonly IServerConfigRepository _serverConfigRepository;
    
    public ServerConfigService(IServerConfigRepository serverConfigRepository)
    {
        _serverConfigRepository = serverConfigRepository;
    }

    public void UpdateServerConfig(ServerConfig serverConfig, string missionNameValue, string hostNameValue, int instanceIdValue, int steamPortValue, int steamQueryPortValue)
    {
        var template = serverConfig.GetPropertyValue("template");
        if (template != null)
        {
            template.Value = missionNameValue;
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue(serverConfig.GetNextID(), "template", DataType.Text, missionNameValue, ""));
        }
        
        var hostName = serverConfig.GetPropertyValue("hostname");
        if (hostName != null)
        {
            hostName.Value = hostNameValue;
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue(serverConfig.GetNextID(), "hostname", DataType.Text, hostNameValue, ""));
        }

        var instanceId = serverConfig.GetPropertyValue("instanceId");
        if (instanceId != null)
        {
            instanceId.Value = instanceIdValue;
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue(serverConfig.GetNextID(), "instanceId", DataType.Text, instanceIdValue, ""));
        }

        var steamPort = serverConfig.GetPropertyValue("steamPort");
        if (steamPort != null)
        {
            steamPort.Value = steamPortValue;
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue(serverConfig.GetNextID(), "steamQueryPort", DataType.Text, steamQueryPortValue, ""));
        }

        var steamQueryPort = serverConfig.GetPropertyValue("steamQueryPort");
        if (steamQueryPort != null)
        {
            steamQueryPort.Value = steamQueryPortValue;
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue(serverConfig.GetNextID(), "steamQueryPort", DataType.Text, steamQueryPortValue, ""));
        }
    }

    public ServerConfig Get(string configPath)
    {
        return _serverConfigRepository.Get(configPath);
    }

    public void Save(ServerConfig serverConfig, string configPath)
    {
        _serverConfigRepository.Save(serverConfig, configPath);
    }
}