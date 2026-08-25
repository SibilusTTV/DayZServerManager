using System.Globalization;
using System.Text.RegularExpressions;
using Domain.ServerConfig;

namespace Application.IRepository;

public interface IServerConfigSerializerRepository
{
    public ServerConfig Deserialize(string config);
    public string Serialize(ServerConfig cfg);
}