using System.Net;
using Domain.Manager;

namespace Application.IService;

public interface IModService
{
    public Mod? Get(string id);
    public Mod? GetByWorkshopId(long workshopId);
    public List<Mod> GetMods();
    public HttpStatusCode RemoveMod(string id);
}