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
    public List<Player> GetPlayers()
    {
        return _playersService.GetPlayers();
    }
    
    [HttpGet]
    public Player? GetPlayer(string id)
    {
        return _playersService.GetPlayer(id);
    }

    [HttpGet]
    public List<ServerPlayerInformation> GetServerPlayerInformation(string id)
    {
        return _playersService.GetServerPlayerInformation(id);
    }

    [HttpPost]
    public IActionResult CreateServerPlayer(string playerId, string instanceId, bool isWhitelisted, bool isBanned)
    {
        return StatusCode((int)_playersService.CreateServerPlayer(playerId, instanceId, isWhitelisted, isBanned));
    }
}