using System.Xml.Serialization;

namespace Domain.Mission.Environment;

[XmlRoot("agent")]
public class AgentItem
{
    [XmlAttribute("type")]
    public string? Type { get; set; }

    [XmlAttribute("chance")]
    public string? Chance { get; set; }

    [XmlElement("spawn")]
    public List<SpawnItem>? SpawnItems { get; set; }
}
