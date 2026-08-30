using System.Net;
using Application.IRepository;
using Application.IService;
using Domain.Profile;
using Domain.Scheduler;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> _logger;
    private readonly IPlayerRepository _playerRepository;
    private readonly IInstanceRepository _instanceRepository;
    
    public PlayerService(ILogger<PlayerService> logger, IPlayerRepository playerRepository, IInstanceRepository instanceRepository)
    {
        _logger = logger;
        _playerRepository = playerRepository;
        _instanceRepository = instanceRepository;
    }

    public List<User> GetPlayers()
    {
        return _playerRepository.GetAllPlayers();
    }

    public User? GetPlayer(string id)
    {
        return _playerRepository.GetPlayer(id);
    }

    public List<ServerPlayerInformation> GetServerPlayerInformation(string id)
    {
        return _playerRepository.GetServerPlayerInformationForInstance(id);
    }

    public HttpStatusCode CreateServerPlayer(string playerId, string instanceId, bool isWhitelisted, bool isBanned, string roleName)
    {
        var role = GetRole(roleName, instanceId);
        
        if (role == null)
        {
            AddRole(roleName, instanceId);
            role = GetRole(roleName, instanceId);
            if (role == null) return HttpStatusCode.InternalServerError;
        }
        
        var serverPlayer = new ServerPlayer(
            instanceId,
            playerId,
            isWhitelisted,
            isBanned,
            role.Id);
        
        return _playerRepository.CreateEditServerPlayer(serverPlayer);
    }

    public List<Role> GetRoles(string instanceId)
    {
        return _playerRepository.GetRoles(instanceId);
    }

    public List<string> GetRoleNames(string instanceId)
    {
        return _playerRepository.GetRoleNames(instanceId);
    }

    public Role? GetRole(string name, string instanceId)
    {
        return _playerRepository.GetRole(name, instanceId);
    }

    public HttpStatusCode AddRole(string name, string instanceId)
    {
        return _playerRepository.AddRole(name, instanceId);
    }

    public void ReadOutRoles(string instanceId)
    {
        var instance = _instanceRepository.GetInstance(instanceId);

        if (instance == null) return;
        
        _playerRepository.ReadOutRoles(Path.Combine(instance.serverFolder, instance.profileName), instanceId);
    }

    public Dictionary<string, PlayerPermissions> ReadOutServerPlayerRoles(string instanceId)
    {
        var instance = _instanceRepository.GetInstance(instanceId);

        if (instance == null) return new Dictionary<string, PlayerPermissions>();
        
        return _playerRepository.ReadOutServerPlayerRoles(Path.Combine(instance.serverFolder, instance.profileName), instanceId);
    }

    public HttpStatusCode SaveServerPlayerRole(string serverPlayerId, string playerGuid, string instanceId, string roleName)
    {
        var instance = _instanceRepository.GetInstance(instanceId);
        if (instance == null) return HttpStatusCode.NotFound;
        
        var role = GetRole(roleName, instanceId);
        if (role == null) return HttpStatusCode.NotFound;
        
        var serverPlayer = _playerRepository.GetServerPlayer(serverPlayerId);
        if (serverPlayer == null)
        {
            var player = GetPlayer(playerGuid);
            if (player == null) return HttpStatusCode.BadRequest;
            
            serverPlayer = new ServerPlayer(instanceId, playerGuid, false, false, role.Id);
        }
        else
        {
            serverPlayer.Role = role;
            serverPlayer.RoleId = role.Id;
        }

        return _playerRepository.SaveServerPlayerRole(Path.Combine(instance.serverFolder, instance.profileName),
            serverPlayer, roleName);
    }
}