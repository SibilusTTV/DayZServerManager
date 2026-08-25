using System.Net;
using Application.IRepository;
using Application.IService;
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

    public List<Player> GetPlayers()
    {
        return _playerRepository.GetAllPlayers();
    }

    public Player? GetPlayer(string id)
    {
        return _playerRepository.GetPlayer(id);
    }

    public List<ServerPlayerInformation> GetServerPlayerInformation(string id)
    {
        return _playerRepository.GetServerPlayerInformationForInstance(id);
    }

    public HttpStatusCode CreateServerPlayer(string playerId, string instanceId, bool isWhitelisted, bool isBanned)
    {
        var serverPlayer = new ServerPlayer(
            instanceId,
            playerId,
            isWhitelisted,
            isBanned,
            "everyone");
        
        return _playerRepository.CreateEditServerPlayer(serverPlayer);
    }
}