namespace Application.IRepository;

public interface IJsonSerializerRepository
{
    public void SerializeJSONFile<JSONFile>(string path, JSONFile jsonfile);
    public JSONFile? DeserializeJSONFile<JSONFile>(string path);
}