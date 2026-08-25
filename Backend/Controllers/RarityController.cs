using Application.IService;
using Domain.Mission.RarityFile;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class RarityController : ControllerBase
{
    private readonly ILogger<InstanceController> _logger;
    private readonly IRarityService _rarityService;

    public RarityController(ILogger<InstanceController> logger, IRarityService rarityService)
    {
        _logger = logger;
        _rarityService = rarityService;
    }

    [HttpGet]
    public RarityFile? Get(Guid instanceId, string name)
    {
        return _rarityService.Get(instanceId, name);
    }

    [HttpPut]
    public void Update(Guid instanceId, string name, RarityFile rarityFile)
    {
        _rarityService.UpdateRaritiesAndTypes(instanceId, name, rarityFile);
    }
}