using Application.IRepository;
using Application.IService;
using Domain.Scheduler;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> _logger;
    private readonly IPlayerRepository _playerRepository;
    public PlayerService(ILogger<PlayerService> logger, IPlayerRepository playerRepository)
    {
        _logger = logger;
        _playerRepository = playerRepository;
    }

    public List<Player> GetPlayers()
    {
        return _playerRepository.GetAllPlayers();
    }
}