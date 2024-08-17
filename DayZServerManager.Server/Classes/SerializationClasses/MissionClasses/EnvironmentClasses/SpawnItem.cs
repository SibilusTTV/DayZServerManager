using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EnvironmentClasses
{
    [XmlRoot("spawn")]
    public class SpawnItem
    {
        [XmlAttribute("configName")]
        public string? ConfigName { get; set; }

        [XmlAttribute("chance")]
        public string? Chance { get; set; }
    }
}
