using System.Xml.Serialization;

namespace Domain.Mission.Environment;

[XmlRoot("item")]
public class ItemElement
{
    [XmlAttribute("name")]
    public string? Name { get; set; }

    [XmlAttribute("val")]
    public string? Val {  get; set; }
}
