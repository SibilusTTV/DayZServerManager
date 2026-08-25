namespace Application.IRepository;

public interface IXmlSerializerRepository
{
    public void SerializeXMLFile<XMLFile>(string path, XMLFile? xmlFile);
    public XMLFile? DeserializeXMLFile<XMLFile>(string path);
}