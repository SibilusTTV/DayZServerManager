using System.Xml.Serialization;
using Application.IRepository;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository;

public class XmlSerializerRepository : IXmlSerializerRepository
{
    private readonly ILogger<XmlSerializerRepository> _logger;
    
    public XmlSerializerRepository(ILogger<XmlSerializerRepository> logger)
    {
        _logger = logger;
    }

    public void SerializeXMLFile<XMLFile>(string path, XMLFile? xmlFile)
    {
        try
        {
            if (xmlFile != null)
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    var fileSerializer = new XmlSerializer(typeof(XMLFile));
                    fileSerializer.Serialize(writer, xmlFile);
                    writer.Close();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when serializing xml file");
        }
    }


    // Takes a path and returns the deserialized TypesFile
    public XMLFile? DeserializeXMLFile<XMLFile>(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using var reader = new StreamReader(path);
                var serializer = new XmlSerializer(typeof(XMLFile));
                return (XMLFile?)serializer.Deserialize(reader);
            }
            else
            {
                return default(XMLFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when deserializing xml file");
            return default(XMLFile);
        }
    }
}