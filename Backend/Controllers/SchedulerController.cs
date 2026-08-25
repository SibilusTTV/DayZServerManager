using System.Net;
using Application.IService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SchedulerController : ControllerBase
{
    private readonly ILogger<SchedulerController> _logger;
    private readonly ISchedulerService _schedulerService;

    public SchedulerController(ILogger<SchedulerController> logger, ISchedulerService schedulerService)
    {
        _logger = logger;
        _schedulerService = schedulerService;
    }

    [HttpGet]
    public IActionResult BanPlayer(string serverPlayerId, string reason, int duration)
    {
        return StatusCode((int)_schedulerService.BanPlayer(serverPlayerId, reason, duration));
    }
    
    [HttpGet]
    public IActionResult WhitelistPlayer(string serverPlayerId, string reason)
    {
        return StatusCode((int)_schedulerService.WhitelistPlayer(serverPlayerId, reason));
    }
}