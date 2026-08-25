using System.Net;
using Application.IRepository;
using Domain.Scheduler;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class PlayerRepository : IPlayerRepository
{
    private readonly ILogger<PlayerRepository> _logger;
    private readonly ConfigDbContext _configDbContext;
    
    public PlayerRepository(ILogger<PlayerRepository> logger, ConfigDbContext configDbContext)
    {
        _logger = logger;
        _configDbContext = configDbContext;
    }

    public List<Player> GetAllPlayers()
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.PLAYERS
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting players");
                return [];
            }
        }
    }

    public Player? GetPlayer(string id)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.PLAYERS
                    .AsNoTracking()
                    .ToArray()
                    .FirstOrDefault(x => x.Guid == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public List<Player> GetPlayersByName(string name)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.PLAYERS
                    .AsNoTracking()
                    .Where(x => x.Name == name)
                    .ToList<Player>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public List<string> GetWhitelistedPlayerNames(string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .Include(x => x.Player)
                    .Where(x => x.InstanceId == instanceId && x.IsWhitelisted)
                    .Select(x => x.Player.Uid)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting whitelisted player names");
                return [];
            }
        }
    }

    public void WhitelistPlayer(string serverPlayerId)
    {
        lock (_configDbContext)
        {
            try
            {
                var playerDb = _configDbContext.SERVER_PLAYERS
                    .ToArray()
                    .FirstOrDefault(x => x.Id == serverPlayerId);
                if (playerDb == null) return;
                playerDb.IsWhitelisted = true;
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error whitelisting player");
            }
        }
    }

    public void UnWhitelistPlayer(string serverPlayerId)
    {
        lock (_configDbContext)
        {
            try
            {
                var playerDb = _configDbContext.SERVER_PLAYERS.FirstOrDefault(x => x.Id == serverPlayerId);
                if (playerDb == null) return;
                playerDb.IsWhitelisted = false;
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error whitelisting player");
            }
        }
    }

    public void CreateEditPlayer(Player player)
    {
        lock (_configDbContext)
        {
            try
            {
                var playerDB = _configDbContext.PLAYERS.FirstOrDefault(x => x.Guid == player.Guid);
                if (playerDB == null)
                {
                    _configDbContext.PLAYERS.Add(player);
                }
                else
                {
                    _configDbContext.Entry(playerDB).CurrentValues.SetValues(player);
                }
                
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding player");
            }
        }
    }

    public List<ServerPlayer> GetServerPlayersForInstance(string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .AsNoTracking()
                    .Where(x => x.InstanceId == instanceId)
                    .Include(x => x.Player)
                    .Include(x => x.Ban)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting server players");
                return [];
            }
        }
    }

    public ServerPlayer? GetServerPlayer(string playerId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .AsNoTracking()
                    .ToArray()
                    .FirstOrDefault(x => x.Id == playerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public List<ServerPlayerInformation> GetServerPlayerInformationForInstance(string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                var result =
                    from player in _configDbContext.PLAYERS
                    join sp in _configDbContext.SERVER_PLAYERS
                            .Where(sp => sp.InstanceId == instanceId)
                        on player.Guid equals sp.PlayerId into spGroup
                    from sp in spGroup.DefaultIfEmpty()
                    select new ServerPlayerInformation(
                        player.Guid,
                        sp != null ? sp.Id : null,
                        player.Name,
                        player.Uid,
                        player.Ip,
                        player.IsVerified,
                        sp != null && sp.IsWhitelisted,
                        sp != null && sp.IsBanned,
                        sp != null ? sp.Role : "",
                        sp != null ? sp.InstanceId : null
                    );
                
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting server players");
                return [];
            }
        }
    }
    
    public HttpStatusCode CreateEditServerPlayer(ServerPlayer player)
    {
        lock (_configDbContext)
        {
            try
            {
                var playerDB = _configDbContext.SERVER_PLAYERS
                    .Include(x => x.Player)
                    .Include(x => x.Ban)
                    .FirstOrDefault(x => x.Id == player.Id);
                
                if (playerDB == null)
                {
                    _configDbContext.SERVER_PLAYERS.Add(player);
                    _configDbContext.SaveChanges();
                    return HttpStatusCode.Created;
                }
                else
                {
                    _configDbContext.Entry(playerDB).CurrentValues.SetValues(player);
                    _configDbContext.SaveChanges();
                    if (player.Ban == null && playerDB.Ban != null)
                    {
                        var ban = _configDbContext.BANS.FirstOrDefault(x => x.Id == playerDB.Ban.Id);
                        if (ban == null) return HttpStatusCode.NotFound;
                        _configDbContext.BANS.Remove(ban);
                        _configDbContext.SaveChanges();
                    }

                    return HttpStatusCode.OK;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving player");
                return HttpStatusCode.InternalServerError;
            }
        }
    }

    public void ClearBans()
    {
        lock (_configDbContext)
        {
            try
            {
                foreach (var ban in _configDbContext.BANS)
                {
                    _configDbContext.BANS.Remove(ban);
                }
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving bans");
            }
        }
    }

    public ServerPlayer? GetBannedServerPlayer(int banId, string guid, string reason)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .AsNoTracking()
                    .Include(x => x.Player)
                    .Include(x => x.Ban)
                    .FirstOrDefault(x => x.Player.Guid == guid && x.Ban != null && x.Ban.BanId == banId && x.Ban.Reason == reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public void CreateNewBan(int banId, string guid, int remainingTime, string reason)
    {
        lock (_configDbContext)
        {
            try
            {
                var player =  _configDbContext.SERVER_PLAYERS
                    .Include(x => x.Player)
                    .Include(x => x.Ban)
                    .FirstOrDefault(x => x.Player.Guid == guid);

                if (player == null) return;

                player.Ban = new Ban(banId, remainingTime, reason);
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new ban");
            }
        }
    }

    public HttpStatusCode RemoveBan(string id)
    {
        lock (_configDbContext)
        {
            try
            {
                var ban = _configDbContext.BANS.FirstOrDefault(x => x.Id == id);
                if (ban == null) return HttpStatusCode.NotFound;
                _configDbContext.BANS.Remove(ban);
                _configDbContext.SaveChanges();
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing ban");
                return HttpStatusCode.InternalServerError;
            }
        }
    }
    
    public void UpdateRemainingTime(string id, int remainingTime)
    {
        lock (_configDbContext)
        {
            try
            {
                var ban = _configDbContext.BANS.FirstOrDefault(x => x.Id == id);
                if (ban == null) return;
                ban.RemainingTime = remainingTime;
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating remaining time");
            }
        }
    }
}