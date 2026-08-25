using Domain.Scheduler;

namespace Application.IRepository;

public interface ISchedulerRepository
{
    public SchedulerConfig? Get(Guid id);
    public void CreateEdit(SchedulerConfig config);
    public List<string> LoadWhitelistedPlayers(string serverFolderName);
    public void SaveWhitelistedPlayers(string serverFolderName, List<string> whitelistedPlayers);
}