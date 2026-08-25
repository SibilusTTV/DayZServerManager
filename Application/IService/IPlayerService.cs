using Domain.Scheduler;

namespace Application.IService;

public interface IPlayerService
{

    public List<Player> GetPlayers();
}