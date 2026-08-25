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

    public Instance? GetInstance(Guid id)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.INSTANCES
                    .AsNoTracking()
                    .Include(instance => instance.clientMods)
                    .Include(instance => instance.serverMods)
                    .Include(instance => instance.customMessages)
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
                    .Include(instance => instance.clientMods)
                    .Include(instance => instance.serverMods)
                    .Include(instance => instance.customMessages)
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
                    .Include(i => i.clientMods)
                    .Include(i => i.serverMods)
                    .Include(i => i.customMessages)
                    .FirstOrDefault(x => x.id == instance.id);
                
                if (instanceDb != null) return HttpStatusCode.BadRequest;
                
                var clientMods = instance.clientMods;
                var serverMods = instance.serverMods;

                instance.clientMods = [];
                instance.serverMods = [];
                
                _configDbContext.INSTANCES.Add(instance);
                _configDbContext.SaveChanges();
                
                instanceDb = _configDbContext.INSTANCES
                    .Include(i => i.clientMods)
                    .Include(i => i.serverMods)
                    .Include(i => i.customMessages)
                    .FirstOrDefault(x => x.id == instance.id);
                
                if (instanceDb == null) return HttpStatusCode.InternalServerError;

                foreach (var clientMod in clientMods)
                {
                    var modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == clientMod.workshopID);
                
                    if (modDb != null)
                    {
                        instanceDb.clientMods.Add(modDb);
                    }
                    else
                    {
                        _configDbContext.MODS.Add(clientMod);
                        
                        _configDbContext.SaveChanges();
                        
                        modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == clientMod.workshopID);
                        
                        if (modDb == null) continue;
                        
                        instanceDb.clientMods.Add(modDb);
                    }
                }
                
                foreach (var serverMod in serverMods)
                {
                    var modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == serverMod.workshopID);
                
                    if (modDb != null)
                    {
                        instanceDb.serverMods.Add(modDb);
                    }
                    else
                    {
                        _configDbContext.MODS.Add(serverMod);
                        
                        _configDbContext.SaveChanges();
                        
                        modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == serverMod.workshopID);
                        
                        if (modDb == null) continue;
                        
                        instanceDb.serverMods.Add(modDb);
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
                    .Include(i => i.serverMods)
                    .Include(i => i.customMessages)
                    .FirstOrDefault(x => x.id == instance.id);
                
                if (instanceDb == null) return HttpStatusCode.NotFound;
                
                _configDbContext.Entry(instanceDb).CurrentValues.SetValues(instance);
                
                #region ClientMods
                foreach (var instanceMod in instance.clientMods)
                {
                    var instanceModDb = instanceDb.clientMods.FirstOrDefault(x => x.id == instanceMod.id);
                    if (instanceModDb == null)
                    {
                        var modDb = _configDbContext.MODS.FirstOrDefault(x => x.id == instanceMod.id);
                        if (modDb != null)
                        {
                            instanceDb.clientMods.Add(modDb);
                        }
                        else
                        {
                            _configDbContext.MODS.Add(instanceMod);
                        
                            _configDbContext.SaveChanges();
                        
                            modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == instanceMod.workshopID);
                        
                            if (modDb == null) continue;
                        
                            instanceDb.serverMods.Add(modDb);
                        }
                    }
                    else
                    {
                        _configDbContext.Entry(instanceModDb).CurrentValues.SetValues(instanceMod);
                    }
                }
                
                var clientModsDbCopy = new List<Mod>(instanceDb.clientMods);
                foreach (var modDbCopy in clientModsDbCopy)
                {
                    if (instance.clientMods.All(x => x.id != modDbCopy.id))
                    {
                        var modDb = instanceDb.clientMods.FirstOrDefault(x => x.id == modDbCopy.id);
                        if (modDb != null) instanceDb.clientMods.Remove(modDb);
                    }
                }
                #endregion ClientMods
                
                #region ServerMods
                foreach (var instanceMod in instance.serverMods)
                {
                    var instanceModDb = instanceDb.serverMods.FirstOrDefault(x => x.id == instanceMod.id);
                    if (instanceModDb == null)
                    {
                        var modDb = _configDbContext.MODS.FirstOrDefault(x => x.id == instanceMod.id);
                        if (modDb != null)
                        {
                            instanceDb.clientMods.Add(modDb);
                        }
                        else
                        {
                            _configDbContext.MODS.Add(instanceMod);
                        
                            _configDbContext.SaveChanges();
                        
                            modDb = _configDbContext.MODS.FirstOrDefault(x => x.workshopID == instanceMod.workshopID);
                        
                            if (modDb == null) continue;
                        
                            instanceDb.serverMods.Add(modDb);
                        }
                    }
                    else
                    {
                        _configDbContext.Entry(instanceModDb).CurrentValues.SetValues(instanceMod);
                    }
                }
                
                var serverModsDb = new List<Mod>(instanceDb.serverMods);
                foreach (var mod in serverModsDb)
                {
                    if (instance.serverMods.All(x => x.id != mod.id))
                    {
                        var modDb = instanceDb.serverMods.FirstOrDefault(x => x.id == mod.id);
                        if (modDb != null) instanceDb.serverMods.Remove(modDb);
                    }
                }
                #endregion ServerMods
                
                #region CustomMessages
                foreach (var customMessage in instance.customMessages)
                {
                    var customMessageDb = instanceDb.customMessages.FirstOrDefault(x => x.Id == customMessage.Id);
                    if (customMessageDb == null)
                    {
                        instanceDb.customMessages.Add(customMessage);
                    }
                    else
                    {
                        _configDbContext.Entry(customMessageDb).CurrentValues.SetValues(customMessage);
                    }
                }
                
                var customMessagesCopy = new List<CustomMessage>(instanceDb.customMessages);
                foreach (var customMessageCopy in customMessagesCopy)
                {
                    if (instance.customMessages.All(x => x.Id != customMessageCopy.Id))
                    {
                        var customMessageDb = instanceDb.customMessages.FirstOrDefault(x => x.Id == customMessageCopy.Id);
                        if (customMessageDb != null) instanceDb.customMessages.Remove(customMessageDb);
                    }
                }
                #endregion CustomMessages
                
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

    public HttpStatusCode DeleteInstance(Guid id)
    {
        lock (_configDbContext)
        {
            try
            {
                var instanceDb = _configDbContext.INSTANCES
                    .Include(x => x.clientMods)
                    .Include(x => x.serverMods)
                    .Include(x => x.customMessages)
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