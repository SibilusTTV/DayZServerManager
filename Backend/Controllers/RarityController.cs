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
    private readonly IInstanceService _instanceService;

    public RarityController(ILogger<InstanceController> logger, IRarityService rarityService, IInstanceService instanceService)
    {
        _logger = logger;
        _rarityService = rarityService;
        _instanceService = instanceService;
    }

    [HttpGet]
    public RarityFile? Get(int instanceId, string name)
    {
        return _rarityService.Get(instanceId, name);
    }

    [HttpPut]
    public void Update(int instanceId, string name, RarityFile rarityFile)
    {
        _rarityService.UpdateRaritiesAndTypes(instanceId, name, rarityFile);
        _instanceService.SetMissionNeedsUpdatingForServer(instanceId);
    }
}