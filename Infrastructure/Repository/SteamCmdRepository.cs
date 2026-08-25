using Application.IRepository;
using Domain.Manager;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class SteamCmdRepository : ISteamCmdRepository
{
    private readonly ILogger<SteamCmdRepository> _logger;
    private readonly ConfigDbContext _configDbContext;
    
    public SteamCmdRepository(ILogger<SteamCmdRepository> logger, ConfigDbContext configDbContext)
    {
        _logger = logger;
        _configDbContext = configDbContext;
    }

    public SteamCredentials? GetCredentials()
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.STEAM_CREDENTIALS
                    .AsNoTracking()
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credentials");
                return null;
            }
        }
    }

    public string GetSteamUsername()
    {
        lock (_configDbContext)
        {
            try
            {
                var credentials = _configDbContext.STEAM_CREDENTIALS
                    .AsNoTracking()
                    .FirstOrDefault();
                
                return credentials != null ? credentials.SteamUsername : "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credentials");
                return "";
            }
        }
    }

    public string GetSteamPassword()
    {
        lock (_configDbContext)
        {
            try
            {
                var credentials = _configDbContext.STEAM_CREDENTIALS
                    .AsNoTracking()
                    .FirstOrDefault();
                
                return credentials != null ? credentials.SteamPassword : "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credentials");
                return "";
            }
        }
    }

    public void SaveCredentials(SteamCredentials credentials)
    {
        lock (_configDbContext)
        {
            try
            {
                var oldCredentials = _configDbContext.STEAM_CREDENTIALS.FirstOrDefault();
                if (oldCredentials != null)
                {
                    oldCredentials.SteamUsername = credentials.SteamUsername;
                    oldCredentials.SteamPassword = credentials.SteamPassword;
                }
                else
                {
                    _configDbContext.STEAM_CREDENTIALS.Add(credentials);
                }
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving credentials");
            }
        }
    }
}