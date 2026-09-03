using System.Net;
using Application.IRepository;
using Domain.Constants;
using Domain.Scheduler;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class SchedulerRepository : ISchedulerRepository
{
    private readonly ILogger<SchedulerRepository> _logger;
    private readonly IJsonSerializerRepository _jsonSerializerRepository;
    private readonly ConfigDbContext _configDbContext;
    
    public SchedulerRepository(ILogger<SchedulerRepository> logger, IJsonSerializerRepository jsonSerializerRepository, ConfigDbContext configDbContext)
    {
        _logger = logger;
        _jsonSerializerRepository = jsonSerializerRepository;
        _configDbContext = configDbContext;
    }

    public SchedulerConfig? Get(int instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SCHEDULER_CONFIGS
                    .AsNoTracking()
                    .Include(schedulerConfig => schedulerConfig.customMessages
                        .OrderBy(x => x.Position))
                    .FirstOrDefault(s => s.InstanceId == instanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scheduler config");
                return null;
            }
        }
    }

    public void CreateEdit(SchedulerConfig config)
    {
        lock (_configDbContext)
        {
            try
            {
                var schedulerConfigDb = _configDbContext.SCHEDULER_CONFIGS
                    .Include(schedulerConfig => schedulerConfig.customMessages)
                    .FirstOrDefault(s => s.InstanceId == config.InstanceId);
                
                if (schedulerConfigDb == null)
                {
                    _configDbContext.SCHEDULER_CONFIGS.Add(config);
                }
                else
                {
                    _configDbContext.Entry(schedulerConfigDb).CurrentValues.SetValues(config);
                
                    var targetCustomMessages  = config.customMessages.Select(x => x.Id).ToHashSet();
                    schedulerConfigDb.customMessages.RemoveAll(m => !targetCustomMessages.Contains(m.Id));
                
                    #region CustomMessages
                    foreach (var customMessage in config.customMessages)
                    {
                        var customMessageDb = schedulerConfigDb.customMessages.FirstOrDefault(x => x.Id == customMessage.Id);
                        if (customMessageDb == null)
                        {
                            schedulerConfigDb.customMessages.Add(customMessage);
                        }
                        else
                        {
                            _configDbContext.Entry(customMessageDb).CurrentValues.SetValues(customMessage);
                        }
                    }
                    #endregion CustomMessages
                
                    _configDbContext.SaveChanges();
                }
                
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving scheduler config");
            }
        }
    }
    
    public List<string> LoadWhitelistedPlayers(string serverFolderName)
    {
        List<string> whitelistedPlayers = [];
        
        try
        {
            if (File.Exists(Path.Combine(serverFolderName, Files.WhitelistFileName)))
            {
                using var reader = new StreamReader(Path.Combine(serverFolderName, Files.WhitelistFileName));
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != null && !string.IsNullOrEmpty(line) && !whitelistedPlayers.Contains(line))
                    {
                        whitelistedPlayers.Add(line);
                    }
                }
            }
            else
            {
                if (!Directory.Exists(serverFolderName))
                {
                    Directory.CreateDirectory(serverFolderName);
                }
                using (var writer = File.Create(Path.Combine(serverFolderName, Files.WhitelistFileName)))
                {
    
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when loading the whitelisted players");
        }
    
        return whitelistedPlayers;
    }

    public HttpStatusCode SaveWhitelistedPlayers(string serverFolderName, List<string> whitelistedPlayers)
    {
        try
        {
            using var writer = new StreamWriter(Path.Combine(serverFolderName, Files.WhitelistFileName));
            foreach (var whitelistedPlayer in whitelistedPlayers)
            {
                if (whitelistedPlayer != string.Empty)
                {
                    writer.WriteLine(whitelistedPlayer);
                }
            }

            return HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when saving the whitelisted players");
            return HttpStatusCode.InternalServerError;
        }
    }
}