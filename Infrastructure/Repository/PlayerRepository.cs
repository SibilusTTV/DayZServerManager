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

    public List<User> GetAllPlayers()
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

    public User? GetPlayer(string id)
    {
        lock (_configDbContext)
        {
            try
            {
                var players = _configDbContext.PLAYERS
                    .AsNoTracking()
                    .ToList();
                
                return players.FirstOrDefault(x => x.Guid == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public List<User> GetPlayersByName(string name)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.PLAYERS
                    .AsNoTracking()
                    .Where(x => x.Name == name)
                    .ToList<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public List<string> GetWhitelistedPlayerNames(int instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .Include(x => x.User)
                    .Where(x => x.InstanceId == instanceId && x.IsWhitelisted)
                    .Select(x => x.User.Uid)
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

    public void CreateEditPlayer(User user)
    {
        lock (_configDbContext)
        {
            try
            {
                var playerDB = _configDbContext.PLAYERS.FirstOrDefault(x => x.Guid == user.Guid);
                if (playerDB == null)
                {
                    _configDbContext.PLAYERS.Add(user);
                }
                else
                {
                    _configDbContext.Entry(playerDB).CurrentValues.SetValues(user);
                }
                
                _configDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding player");
            }
        }
    }

    public List<ServerPlayer> GetServerPlayersForInstance(int instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .AsNoTracking()
                    .Where(x => x.InstanceId == instanceId)
                    .Include(x => x.User)
                    .Include(x => x.Role)
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
                    .Include(x => x.User)
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

    public ServerPlayer? GetServerPlayerByGuid(string playerGuid, int instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .Include(x => x.User)
                    .Include(x => x.Role)
                    .AsNoTracking()
                    .FirstOrDefault(x => x.InstanceId == instanceId && x.User.Guid == playerGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player");
                return null;
            }
        }
    }

    public List<ServerPlayerInformation> GetServerPlayerInformationForInstance(int instanceId)
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
                    .Include(x => x.User)
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

    public List<ServerPlayer> GetBannedServerPlayers(int instanceId)
    {
        lock (_configDbContext)
        {
            try
            {
                return _configDbContext.SERVER_PLAYERS
                    .Include(x => x.User)
                    .Include(x => x.Role)
                    .Where(x => x.IsBanned && x.InstanceId == instanceId)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banned players");
                return [];
            }
        }
    }

    public List<Role> GetRoles(int instanceId)
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

    public List<string> GetRoleNames(int instanceId)
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

    public Role? GetRole(string name, int instanceId)
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

    public HttpStatusCode AddRole(string name, int instanceId)
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

    public void ReadOutRoles(string profileFolder, int instanceId)
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

    public Dictionary<string, PlayerPermissions> ReadOutServerPlayerRoles(string profileFolder, int instanceId)
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
                Folders.PlayersFolderName));

            var playerPermissionsList = new Dictionary<string, PlayerPermissions>();
            
            foreach (var playerFile in players)
            {
                var playerUid = Path.GetFileNameWithoutExtension(playerFile);
                var playerPermissionsFile = _jsonSerializer.DeserializeJSONFile<PlayerPermissions>(playerFile);
                if (playerPermissionsFile == null || playerPermissionsFile.Roles.Count <= 0) continue;
                
                playerPermissionsList.Add(playerUid, playerPermissionsFile);
            }

            return playerPermissionsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading player roles");
            return new Dictionary<string, PlayerPermissions>();
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
                    serverPlayer.User.Uid + ".json"), playerJsonFile);

            return CreateEditServerPlayer(serverPlayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving player role");
            return HttpStatusCode.InternalServerError;
        }
    }
}