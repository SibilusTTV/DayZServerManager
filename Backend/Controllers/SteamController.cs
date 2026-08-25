using Application.IService;
using Domain.Manager;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SteamController : ControllerBase
{
    private readonly ILogger<ServerConfigController> _logger;
    private readonly ISteamCmdService _steamCmdService;

    public SteamController(ILogger<ServerConfigController> logger, ISteamCmdService steamCmdService)
    {
        _logger = logger;
        _steamCmdService = steamCmdService;
    }

    [HttpGet]
    public SteamInformation GetSteamInformation()
    {
        return _steamCmdService.GetSteamInformation();
    }

    [HttpPost]
    public void WriteSteamGuard([FromBody] string steamGuard)
    {
        _steamCmdService.WriteSteamGuard(steamGuard);
    }

    [HttpGet]
    public SteamCredentials GetSteamCredentials()
    {
        return _steamCmdService.GetSteamCredentials();
    }

    [HttpPost]
    public void SaveSteamCredentials([FromBody] SteamCredentials steamCredentials)
    {
        _steamCmdService.SaveSteamCredentials(steamCredentials);
    }
}