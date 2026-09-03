using System.Net;
using Domain.Scheduler;

namespace Application.IRepository;

public interface ISchedulerRepository
{
    public SchedulerConfig? Get(int id);
    public void CreateEdit(SchedulerConfig config);
    public HttpStatusCode Delete(int instanceId);
    public List<string> LoadWhitelistedPlayers(string serverFolderName);
    public HttpStatusCode SaveWhitelistedPlayers(string serverFolderName, List<string> whitelistedPlayers);
}