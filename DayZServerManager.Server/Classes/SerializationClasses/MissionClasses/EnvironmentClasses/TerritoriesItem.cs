using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EnvironmentClasses
{
    [XmlRoot("territories")]
    public class TerritoriesItem
    {
        [XmlElement("file")]
        public List<EnvironmentFileItem>? Files { get; set; }

        [XmlElement("territory")]
        public List<TerritoryItem>? Territories { get; set; }
    }
}
