using Application.IRepository;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class InitFileSerializerRepository : IInitFileSerializerRepository
{
    private readonly ILogger<InitFileSerializerRepository> _logger;

    public InitFileSerializerRepository(ILogger<InitFileSerializerRepository> logger)
    {
        _logger = logger;
    }
    
    public void SerializeInitFile(string path, string initFile)
    {
        try
        {
            using var writer = new StreamWriter(path);
            writer.Write(initFile);
            writer.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when serializing init file {path}", path);
        }
    }
    
    public string DeserializeInitFile(string path)
    {
        try
        {
            using var reader = new StreamReader(path);
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when deserializing init file {path}", path);
            return string.Empty;
        }
    }
}