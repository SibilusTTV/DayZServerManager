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
}