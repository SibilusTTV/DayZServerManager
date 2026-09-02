using Application.IService;
using Domain.Scheduler;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PlayerController : ControllerBase
{
    private readonly ILogger<PlayerController> _logger;
    private readonly IPlayerService _playersService;

    public PlayerController(ILogger<PlayerController> logger, IPlayerService playersService)
    {
        _logger = logger;
        _playersService = playersService;
    }

    [HttpGet]
    public List<User> GetPlayers()
    {
        return _playersService.GetPlayers();
    }
    
    [HttpGet]
    public User? GetPlayer(string id)
    {
        return _playersService.GetPlayer(id);
    }

    [HttpGet]
    public List<ServerPlayerInformation> GetServerPlayerInformation(int id)
    {
        return _playersService.GetServerPlayerInformation(id);
    }

    [HttpPost]
    public IActionResult CreateServerPlayer(string playerId, int instanceId, bool isWhitelisted, bool isBanned, string roleName)
    {
        return StatusCode((int)_playersService.CreateServerPlayer(playerId, instanceId, isWhitelisted, isBanned, roleName));
    }

    [HttpGet]
    public List<string> GetRoleNames(int instanceId)
    {
        return _playersService.GetRoleNames(instanceId);
    }

    [HttpPost]
    public IActionResult SetRole(string serverPlayerId, string playerGuid, int instanceId, string roleName)
    {
        return StatusCode((int)_playersService.SaveServerPlayerRole(serverPlayerId, playerGuid, instanceId, roleName));
    }
}