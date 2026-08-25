using System.Net;
using Application.IRepository;
using Application.IService;
using Domain.Manager;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class ModService : IModService
{
    private readonly ILogger<ModService> _logger;
    private readonly IModRepository _modRepository;

    public ModService(ILogger<ModService> logger, IModRepository modRepository)
    {
        _logger = logger;
        _modRepository = modRepository;
    }
    
    public Mod? Get(Guid id)
    {
        return _modRepository.Get(id);
    }

    public Mod? GetByWorkshopId(long workshopId)
    {
        return  _modRepository.GetByWorkshopId(workshopId);
    }

    public List<Mod> GetMods()
    {
        return _modRepository.GetMods();
    }

    public HttpStatusCode RemoveMod(Guid id)
    {
        return _modRepository.RemoveMod(id);
    }
}