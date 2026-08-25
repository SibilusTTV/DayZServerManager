using System.Net;
using Domain.Scheduler;

namespace Application.IRepository;

public interface ISchedulerRepository
{
    public SchedulerConfig? Get(string id);
    public void CreateEdit(SchedulerConfig config);
    public List<string> LoadWhitelistedPlayers(string serverFolderName);
    public HttpStatusCode SaveWhitelistedPlayers(string serverFolderName, List<string> whitelistedPlayers);
}