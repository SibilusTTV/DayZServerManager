using Application.IService;
using Application.Service;
using Domain.Constants;
using Domain.Manager;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class InstanceController : ControllerBase
{
    private readonly ILogger<InstanceController> _logger;
    private readonly IInstanceService _instanceService;
    private readonly ISteamCmdService _steamCmdService;

    public InstanceController(ILogger<InstanceController> logger, IInstanceService instanceService, ISteamCmdService steamCmdService)
    {
        _logger = logger;
        _instanceService = instanceService;
        _steamCmdService = steamCmdService;
    }

    [HttpGet]
    public void StartServer(Guid id)
    {
        var steamCmdStatus = _steamCmdService.GetSteamInformation().steamCmdStatus;
        
        if (steamCmdStatus != Statuses.SteamGuard || steamCmdStatus != Statuses.CachedCredentials) _instanceService.StartServer(id);
    }

    [HttpGet]
    public void StopServer(Guid id)
    {
        _instanceService.StopServer(id);
    }

    [HttpDelete]
    public void RemoveServer(Guid id)
    {
        _instanceService.RemoveServer(id);
    }
    
    [HttpGet]
    public ServerInformation? GetServerInformation(Guid id)
    {
        return _instanceService.GetServerInformation(id);
    }

    [HttpGet]
    public Instance? GetInstance(Guid id)
    {
        return _instanceService.GetInstance(id);
    }

    [HttpGet]
    public List<Instance> GetInstances()
    {
        return _instanceService.GetInstances();
    }

    [HttpGet]
    public Instance? CreateEmptyInstance()
    {
        return _instanceService.CreateEmptyInstanceConfig();
    }

    [HttpPost]
    public void CreateServer([FromBody] Instance instance)
    {
        _instanceService.CreateInstance(instance);
        _instanceService.CreateServer(instance);
    }

    [HttpPut]
    public void UpdateInstance([FromBody] Instance instance)
    {
        _instanceService.UpdateInstanceConfig(instance);
    }
}