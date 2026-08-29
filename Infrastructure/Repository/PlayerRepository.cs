using System.Net;
using Application.IRepository;
using Domain.Constants;
using Domain.Profile;
using Domain.Scheduler;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;

namespace Infrastructure.Repository;

public class PlayerRepository : IPlayerRepository
{
    private readonly ILogger<PlayerRepository> _logger;
    private readonly ConfigDbContext _configDbContext;
    private readonly IJsonSerializerRepository _jsonSerializer;
    
    public PlayerRepository(ILogger<PlayerRepository> logger, ConfigDbContext configDbContext, IJsonSerializerRepository jsonSerializer)
    {
        _logger = logger;
        _configDbContext = configDbContext;
        _jsonSerializer = jsonSerializer;
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

    public ServerPlayer? GetServerPlayer(string serverPlayerId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .Include(x => x.Player)
                    .Include(x => x.Ban)
                    .Include(x => x.Role)
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == serverPlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public ServerPlayer? GetServerPlayerByGuid(string playerGuid, string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .Include(x => x.Player)
                    .Include(x => x.Ban)
                    .Include(x => x.Role)
                    .AsNoTracking()
                    .FirstOrDefault(x => x.InstanceId == instanceId && x.Player.Guid == playerGuid);
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
                        sp != null ? sp.Role.Name : "",
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
                    .Include(x => x.Role)
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

    public List<Role> GetRoles(string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.ROLES
                    .Where(x => x.InstanceId == instanceId)
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return [];
            }
        }
    }

    public List<string> GetRoleNames(string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.ROLES
                    .Where(x => x.InstanceId == instanceId)
                    .Select(x => x.Name)
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return [];
            }
        }
    }

    public Role? GetRole(string name, string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.ROLES
                    .Where(x => x.Name == name && x.InstanceId == instanceId)
                    .AsNoTracking()
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role");
                return null;
            }
        }
    }

    public HttpStatusCode AddRole(string name, string instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                var role = _configDbContext.ROLES.FirstOrDefault(x => x.Name == name);
                
                if (role != null) return HttpStatusCode.BadRequest;
                
                _configDbContext.ROLES.Add(new Role(name, instanceId));
                _configDbContext.SaveChanges();
                return HttpStatusCode.Created;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding role");
                return HttpStatusCode.InternalServerError;
            }
        }
    }

    public void ReadOutRoles(string profileFolder, string instanceId)
    {
        try
        {
            if (!Directory.Exists(Path.Combine(profileFolder, Folders.PermissionFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(profileFolder, Folders.PermissionFolderName));
            }
            
            if (!Directory.Exists(Path.Combine(profileFolder, Folders.PermissionFolderName, Folders.RolesFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(profileFolder, Folders.PermissionFolderName,
                    Folders.RolesFolderName));
            }

            var roles = Directory.GetFiles(Path.Combine(profileFolder, Folders.PermissionFolderName,
                Folders.RolesFolderName));

            foreach (var roleFile in roles)
            {
                var roleName = Path.GetFileNameWithoutExtension(roleFile);
                var role = GetRole(roleName, instanceId);
                
                if (role == null) AddRole(roleName, instanceId);
                
                // TODO: Add Logic to update the rights a role has
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading roles");
        }
    }

    public void ReadOutServerPlayerRoles(string profileFolder, string instanceId)
    {
        try
        {
            if (!Directory.Exists(Path.Combine(profileFolder, Folders.PermissionFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(profileFolder, Folders.PermissionFolderName));
            }
            
            if (!Directory.Exists(Path.Combine(profileFolder, Folders.PermissionFolderName, Folders.PlayersFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(profileFolder, Folders.PermissionFolderName,
                    Folders.PlayersFolderName));
            }

            var players = Directory.GetFiles(Path.Combine(profileFolder, Folders.PermissionFolderName,
                Folders.RolesFolderName));
            
            foreach (var playerFile in players)
            {
                var playerGuid = Path.GetFileNameWithoutExtension(playerFile);
                var serverPlayer = GetServerPlayerByGuid(playerGuid, instanceId);
                
                var playerPermissionsFile = _jsonSerializer.DeserializeJSONFile<PlayerPermissions>(playerFile);
                if (playerPermissionsFile == null || playerPermissionsFile.Roles.Count <= 0) return;
                    
                var roleName = playerPermissionsFile.Roles.FirstOrDefault();
                if (roleName == null) return;
                
                var role = GetRole(roleName, instanceId);
                if (role == null) return;

                if (serverPlayer == null)
                {
                    var player = GetPlayer(playerGuid);
                    if (player == null) return;

                    serverPlayer = new ServerPlayer(instanceId, player.Guid, false, false, role.Id);
                }
                else
                {
                    serverPlayer.Role = role;
                }
                
                CreateEditServerPlayer(serverPlayer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading player roles");
        }
    }

    public HttpStatusCode SaveServerPlayerRole(string profileFolder, ServerPlayer serverPlayer, string roleName)
    {
        try
        {
            if (!Directory.Exists(Path.Combine(profileFolder, Folders.PermissionFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(profileFolder, Folders.PermissionFolderName));
            }

            if (!Directory.Exists(Path.Combine(profileFolder, Folders.PermissionFolderName, Folders.PlayersFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(profileFolder, Folders.PermissionFolderName,
                    Folders.PlayersFolderName));
            }
            
            var playerJsonFile = new PlayerPermissions()
            {
                Roles = [roleName]
            };

            _jsonSerializer.SerializeJSONFile(
                Path.Combine(profileFolder, Folders.PermissionFolderName, Folders.PlayersFolderName,
                    serverPlayer.PlayerId + ".json"), playerJsonFile);

            return CreateEditServerPlayer(serverPlayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving player role");
            return HttpStatusCode.InternalServerError;
        }
    }
}