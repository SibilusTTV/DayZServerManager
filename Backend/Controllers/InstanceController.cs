using Application.IService;
using Domain.Constants;
using Domain.Manager;
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
    public void StartServer(string id)
    {
        var steamCmdStatus = _steamCmdService.GetSteamInformation().steamCmdStatus;
        
        if (steamCmdStatus != Statuses.SteamGuard || steamCmdStatus != Statuses.CachedCredentials) _instanceService.StartServer(id);
    }

    [HttpGet]
    public void StopServer(string id)
    {
        _instanceService.StopServer(id);
    }

    [HttpDelete]
    public void RemoveServer(string id)
    {
        _instanceService.RemoveServer(id);
    }
    
    [HttpGet]
    public ServerInformation? GetServerInformation(string id)
    {
        return _instanceService.GetServerInformation(id);
    }

    [HttpGet]
    public Instance? GetInstance(string id)
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

    [HttpGet]
    public IActionResult BanPlayer(string playerGuid, string instanceId, string reason, int duration)
    {
        return StatusCode((int)_instanceService.BanPlayer(playerGuid, instanceId, reason, duration));
    }

    [HttpGet]
    public IActionResult UnbanPlayer(string playerGuid, string instanceId)
    {
        return StatusCode((int)_instanceService.UnbanPlayer(playerGuid, instanceId));
    }

    [HttpGet]
    public void KickPlayer(string playerGuid, string instanceId, string reason)
    {
        _instanceService.KickPlayer(playerGuid, instanceId, reason);
    }

    [HttpGet]
    public void WhitelistPlayer(string playerGuid, string instanceId)
    {
        _instanceService.WhitelistPlayer(playerGuid, instanceId);
    }

    [HttpGet]
    public void UnwhitelistPlayer(string playerGuid, string instanceId)
    {
        _instanceService.UnwhitelistPlayer(playerGuid, instanceId);
    }
}