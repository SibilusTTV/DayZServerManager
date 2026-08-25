using System.Text.Json;
using Application.IRepository;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class JsonSerializerRepository : IJsonSerializerRepository
{
    ILogger<JsonSerializerRepository> _logger;

    public JsonSerializerRepository(ILogger<JsonSerializerRepository> logger)
    {
        _logger = logger;
    }
    
    public void SerializeJSONFile<JSONFile>(string path, JSONFile jsonfile)
    {
        try
        {
            using var writer = new StreamWriter(path);
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            var json = JsonSerializer.Serialize(jsonfile, options);
            writer.Write(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when serializing json file");
        }
    }

    // Takes a path and returns the deserialized class
    public JSONFile? DeserializeJSONFile<JSONFile>(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using var reader = new StreamReader(path);
                var json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<JSONFile>(json);
            }
            else
            {
                return default(JSONFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when deserializing json file");
            return default(JSONFile);
        }
    }
}