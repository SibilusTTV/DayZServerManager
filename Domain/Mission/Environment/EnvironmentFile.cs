using System.Xml.Serialization;

namespace Domain.Mission.Environment;

[XmlRoot("env")]
public class EnvironmentFile
{
    [XmlElement("territories")]
    public TerritoriesItem? Territories {  get; set; }
}
