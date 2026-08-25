using System.Net;
using Application.IRepository;
using Application.IService;
using Domain.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ModController : ControllerBase
{
    private readonly ILogger<InstanceController> _logger;
    private readonly IModService _modService;

    public ModController(ILogger<InstanceController> logger, IModService modService)
    {
        _logger = logger;
        _modService = modService;
    }

    [HttpGet]
    public Mod? Get(Guid id)
    {
        return _modService.Get(id);
    }

    [HttpGet]
    public List<Mod> GetMods()
    {
        return _modService.GetMods();
    }

    [HttpDelete]
    public HttpStatusCode DeleteMod(Guid id)
    {
        return _modService.RemoveMod(id);
    }
}