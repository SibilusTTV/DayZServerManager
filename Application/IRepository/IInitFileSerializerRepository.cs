namespace Application.IRepository;

public interface IInitFileSerializerRepository
{
    public void SerializeInitFile(string path, string initFile);
    public string DeserializeInitFile(string path);
}