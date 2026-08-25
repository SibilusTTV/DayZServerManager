using System.Net;
using Domain.Manager;

namespace Application.IRepository;

public interface IModRepository
{
    public Mod? Get(string id);
    public Mod? GetByWorkshopId(long workshopId);
    public List<Mod> GetMods();
    public HttpStatusCode RemoveMod(string id);
}