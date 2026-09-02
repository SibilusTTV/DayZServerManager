using System.Net;
using Domain.Manager;

namespace Application.IRepository;

public interface IInstanceRepository
{
    public Instance? GetInstance(int id);
    public List<Instance> GetInstances();
    public HttpStatusCode CreateInstance(Instance instance);
    public HttpStatusCode UpdateInstance(Instance instance);
    public HttpStatusCode DeleteInstance(int id);
}