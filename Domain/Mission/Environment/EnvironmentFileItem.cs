using System.Xml.Serialization;

namespace Domain.Mission.Environment;

[XmlRoot("file")]
public class EnvironmentFileItem
{
    [XmlAttribute("path")]
    public string? Path { get; set; }

    [XmlAttribute("usable")]
    public string? Usable { get; set; }
}
