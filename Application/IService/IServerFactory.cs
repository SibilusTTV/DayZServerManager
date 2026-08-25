using Domain.Manager;

namespace Application.IService;

public interface IServerFactory
{
    public IServerInstance CreateServerAsync(string id);
}