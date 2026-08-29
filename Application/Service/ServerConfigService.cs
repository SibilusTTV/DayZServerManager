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
            serverConfig.Properties.Add(new PropertyValue("template", missionNameValue, ""));
        }
        
        var hostName = serverConfig.GetPropertyValue("hostname");
        if (hostName != null)
        {
            hostName.Value = hostNameValue;
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue("hostname", hostNameValue, ""));
        }

        var instanceId = serverConfig.GetPropertyValue("instanceId");
        if (instanceId != null)
        {
            instanceId.Value = instanceIdValue.ToString();
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue("instanceId", instanceIdValue.ToString(), ""));
        }

        var steamPort = serverConfig.GetPropertyValue("steamPort");
        if (steamPort != null)
        {
            steamPort.Value = steamPortValue.ToString();
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue("steamQueryPort", steamQueryPortValue.ToString(), ""));
        }

        var steamQueryPort = serverConfig.GetPropertyValue("steamQueryPort");
        if (steamQueryPort != null)
        {
            steamQueryPort.Value = steamQueryPortValue.ToString();
        }
        else
        {
            serverConfig.Properties.Add(new PropertyValue("steamQueryPort", steamQueryPortValue.ToString(), ""));
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