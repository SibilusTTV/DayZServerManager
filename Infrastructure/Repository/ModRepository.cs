using System.Net;
using Application.IRepository;
using Domain.Manager;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class ModRepository : IModRepository
{
    private readonly ILogger<ModRepository> _logger;
    private readonly ConfigDbContext _configDbContext;
    
    public ModRepository(ILogger<ModRepository> logger, ConfigDbContext configDbContext)
    {
        _logger = logger;
        _configDbContext = configDbContext;
    }

    public Mod? Get(Guid id)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.MODS
                    .AsNoTracking()
                    .FirstOrDefault(x => x.id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mod {id}", id);
                return null;
            }
        }
    }

    public Mod? GetByWorkshopId(long workshopId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.MODS
                    .AsNoTracking()
                    .FirstOrDefault(x => x.workshopID == workshopId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mod {workshopId}", workshopId);
                return null;
            }
        }
    }

    public List<Mod> GetMods()
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.MODS
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mods");
                return [];
            }
        }
    }

    public HttpStatusCode RemoveMod(Guid id)
    {
        lock (_configDbContext)
        {
            try
            {
                var mod = _configDbContext.MODS
                    .FirstOrDefault(x => x.id == id);
                
                if (mod == null) return HttpStatusCode.NotFound;
                
                _configDbContext.MODS.Remove(mod);
                _configDbContext.SaveChanges();
                
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting mod {id}", id);
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}