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

    public Player? GetPlayer(Guid id)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.PLAYERS
                    .AsNoTracking()
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

    public List<string> GetWhitelistedPlayerNames(Guid instanceId)
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

    public void WhitelistPlayer(Guid serverPlayerId)
    {
        lock (_configDbContext)
        {
            try
            {
                var playerDb = _configDbContext.SERVER_PLAYERS.FirstOrDefault(x => x.Id == serverPlayerId);
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

    public void UnWhitelistPlayer(Guid serverPlayerId)
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

    public List<ServerPlayer> GetServerPlayersForInstance(Guid instanceId)
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

    public ServerPlayer? GetServerPlayer(Guid playerId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == playerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public List<ServerPlayerInformation> GetServerPlayerInformationForInstance(Guid instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.PLAYERS
                    .LeftJoin(_configDbContext.SERVER_PLAYERS,
                        player => player.Guid,
                        serverPlayer => serverPlayer.Id,
                        (player, serverPlayer) => new ServerPlayerInformation(
                            player.Guid, (serverPlayer != null ? serverPlayer.Id : Guid.NewGuid()), player.Name, player.Uid, player.Ip, player.IsVerified,
                            (serverPlayer != null && serverPlayer.IsWhitelisted), (serverPlayer != null && serverPlayer.IsBanned),
                            (serverPlayer != null ? serverPlayer.Role : ""), (serverPlayer != null ? serverPlayer.InstanceId : Guid.NewGuid())
                        ))
                    .Where(player => player.InstanceId == instanceId)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting server players");
                return [];
            }
        }
    }
    
    public void CreateEditServerPlayer(ServerPlayer player)
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
                }
                else
                {
                    _configDbContext.Entry(playerDB).CurrentValues.SetValues(player);
                    if (player.Ban == null && playerDB.Ban != null)
                    {
                        var ban = _configDbContext.BANS.FirstOrDefault(x => x.Id == playerDB.Ban.Id);
                        if (ban == null) return;
                        _configDbContext.BANS.Remove(ban);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving player");
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

    public ServerPlayer? GetBannedServerPlayer(int banId, Guid guid, string reason)
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

    public void CreateNewBan(int banId, Guid guid, int remainingTime, string reason)
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

    public void RemoveBan(Guid id)
    {
        lock (_configDbContext)
        {
            try
            {
                var ban = _configDbContext.BANS.FirstOrDefault(x => x.Id == id);
                if (ban == null) return;
                _configDbContext.BANS.Remove(ban);
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing ban");
            }
        }
    }
    
    public void UpdateRemainingTime(Guid id, int remainingTime)
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