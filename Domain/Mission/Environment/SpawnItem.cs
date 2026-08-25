using System.Xml.Serialization;

namespace Domain.Mission.Environment;

[XmlRoot("spawn")]
public class SpawnItem
{
    [XmlAttribute("configName")]
    public string? ConfigName { get; set; }

    [XmlAttribute("chance")]
    public string? Chance { get; set; }
}
