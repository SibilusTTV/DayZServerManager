using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EnvironmentClasses
{
    [XmlRoot("territory")]
    public class TerritoryItem
    {
        [XmlAttribute("type")]
        public string? Type { get; set; }

        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("behavior")]
        public string? Behavior { get; set; }

        [XmlElement("file")]
        public List<EnvironmentFileItem>? File { get; set; }

        [XmlElement("agent")]
        public List<AgentItem>? Agents { get; set; }

        [XmlElement("item")]
        public List<ItemElement>? Items { get; set; }
    }
}
