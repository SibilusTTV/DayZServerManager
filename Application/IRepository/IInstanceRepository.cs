using System.Net;
using Domain.Manager;

namespace Application.IRepository;

public interface IInstanceRepository
{
    public Instance? GetInstance(string id);
    public List<Instance> GetInstances();
    public HttpStatusCode CreateInstance(Instance instance);
    public HttpStatusCode UpdateInstance(Instance instance);
    public HttpStatusCode DeleteInstance(string id);
}