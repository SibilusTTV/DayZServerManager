using System.Net;
using Application.IService;
using Domain.ServerConfig;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ServerConfigController : ControllerBase
{
    private readonly ILogger<ServerConfigController> _logger;
    private readonly IServerConfigService _serverConfigService;
    private readonly IInstanceService _instanceService;

    public ServerConfigController(ILogger<ServerConfigController> logger, IServerConfigService serverConfigService,
        IInstanceService instanceService)
    {
        _logger = logger;
        _serverConfigService = serverConfigService;
        _instanceService = instanceService;
    }

    [HttpGet]
    public ServerConfig Get(Guid instanceId)
    {
        return _instanceService.GetServerConfig(instanceId);
    }

    [HttpPost]
    public HttpStatusCode Post([FromBody] ServerConfig serverConfig, Guid instanceId)
    {
        return _instanceService.SaveServerConfig(serverConfig, instanceId);
    }
}