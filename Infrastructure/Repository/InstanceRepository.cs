using System.Net;
using Application.IRepository;
using Domain.Manager;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class InstanceRepository : IInstanceRepository
{
    private readonly ILogger<InstanceRepository> _logger;
    private readonly ConfigDbContext _configDbContext;
    
    public InstanceRepository(ILogger<InstanceRepository> logger, ConfigDbContext configDbContext)
    {
        _logger = logger;
        _configDbContext = configDbContext;
    }

    public Instance? GetInstance(int id)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.INSTANCES
                    .AsNoTracking()
                    .Include(instance => instance.clientMods
                        .OrderBy(clientMod => clientMod.Position))
                    .ThenInclude(icm => icm.Mod)
                    .Include(instance => instance.serverMods
                        .OrderBy(serverMod => serverMod.Position))
                    .ThenInclude(ism => ism.Mod)
                    .FirstOrDefault(x => x.id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instance {id}", id);
                return null;
            }
        }
    }

    public List<Instance> GetInstances()
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.INSTANCES
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instances");
                return [];
            }
        }
    }

    public HttpStatusCode CreateInstance(Instance instance)
    {
        lock (_configDbContext)
        {
            try
            {
                var instanceDb = _configDbContext.INSTANCES
                    .FirstOrDefault(x => x.id == instance.id);
                
                if (instanceDb != null) return HttpStatusCode.BadRequest;

                List<InstanceClientMod> clientMods = [.. instance.clientMods];
                List<InstanceServerMod> serverMods = [..instance.serverMods];
                
                instance.clientMods = [];
                instance.serverMods = [];
                
                _configDbContext.INSTANCES.Add(instance);
                _configDbContext.SaveChanges();
                
                instanceDb = _configDbContext.INSTANCES
                    .FirstOrDefault(x => x.id == instance.id);
                
                if (instanceDb == null) return HttpStatusCode.InternalServerError;
                
                foreach (var clientMod in clientMods)
                {
                    var modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == clientMod.Mod.workshopID);
                    if (modDb != null)
                    {
                        modDb.name = clientMod.Mod.name;
                        instanceDb.clientMods.Add(new InstanceClientMod(instance.id, modDb, clientMod.Position));
                    }
                    else
                    {
                        instanceDb.clientMods.Add(clientMod);
                    }
                }
                
                _configDbContext.SaveChanges();
                
                foreach (var serverMod in serverMods)
                {
                    var modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == serverMod.Mod.workshopID);
                    if (modDb != null)
                    {
                        modDb.name = serverMod.Mod.name;
                        instanceDb.serverMods.Add(new InstanceServerMod(instance.id, modDb, serverMod.Position));
                    }
                    else
                    {
                        instanceDb.serverMods.Add(serverMod);
                    }
                }
                
                _configDbContext.SaveChanges();
                
                return HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating instance {instance}", instance);
                return HttpStatusCode.InternalServerError;
            }
        }
    }

    public HttpStatusCode UpdateInstance(Instance instance)
    {
        lock (_configDbContext)
        {
            try
            {
                var instanceDb = _configDbContext.INSTANCES
                    .Include(i => i.clientMods)
                    .ThenInclude(icm => icm.Mod)
                    .Include(i => i.serverMods)
                    .ThenInclude(ism => ism.Mod)
                    .AsTracking()
                    .FirstOrDefault(x => x.id == instance.id);
                
                if (instanceDb == null) return HttpStatusCode.NotFound;
                
                _configDbContext.Entry(instanceDb).CurrentValues.SetValues(instance);
                _configDbContext.SaveChanges();
                
                var currentClientMods = instanceDb.clientMods.Select(x => x.Mod.workshopID).ToHashSet();
                var targetClientMods  = instance.clientMods.Select(x => x.Mod.workshopID).ToHashSet();
                
                instanceDb.clientMods.RemoveAll(m => !targetClientMods.Contains(m.Mod.workshopID));
                
                foreach (var clientMod in instance.clientMods)
                {
                    if (!currentClientMods.Contains(clientMod.Mod.workshopID))
                    {
                        var modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == clientMod.Mod.workshopID);
                        if (modDb != null)
                        {
                            modDb.name = clientMod.Mod.name;
                            instanceDb.clientMods.Add(new InstanceClientMod(instance.id, modDb, clientMod.Position));
                        }
                        else
                        {
                            instanceDb.clientMods.Add(clientMod);
                        }
                    }
                    else
                    {
                        var modDb = instanceDb.clientMods.FirstOrDefault(x => x.ModId == clientMod.ModId);
                        modDb?.Mod.name = clientMod.Mod.name;
                        modDb?.Position = clientMod.Position;
                    }
                }
                
                _configDbContext.SaveChanges();
                
                var currentServerMods = instanceDb.serverMods.Select(x => x.Mod.workshopID).ToHashSet();
                var targetServerMods  = instance.serverMods.Select(x => x.Mod.workshopID).ToHashSet();
                
                instanceDb.serverMods.RemoveAll(m => !targetServerMods.Contains(m.Mod.workshopID));
                
                foreach (var serverMod in instance.serverMods)
                {
                    if (!currentServerMods.Contains(serverMod.Mod.workshopID))
                    {
                        var modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == serverMod.Mod.workshopID);
                        if (modDb != null)
                        {
                            modDb.name = serverMod.Mod.name;
                            instanceDb.serverMods.Add(new InstanceServerMod(instance.id, modDb, serverMod.Position));
                        }
                        else
                        {
                            instanceDb.serverMods.Add(serverMod);
                        }
                    }
                    else
                    {
                        var modDb = instanceDb.serverMods.FirstOrDefault(x => x.ModId == serverMod.ModId);
                        modDb?.Mod.name = serverMod.Mod.name;
                        modDb?.Position = serverMod.Position;
                    }
                }
                
                _configDbContext.SaveChanges();
                
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating instance {instance}", instance);
                return HttpStatusCode.InternalServerError;
            }
        }
    }

    public HttpStatusCode DeleteInstance(int id)
    {
        lock (_configDbContext)
        {
            try
            {
                var instanceDb = _configDbContext.INSTANCES
                    .FirstOrDefault(inst => inst.id == id);
                if (instanceDb == null) return HttpStatusCode.NotFound;
                
                _configDbContext.INSTANCES.Remove(instanceDb);
                
                _configDbContext.SaveChanges();
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting instance {id}", id);
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}