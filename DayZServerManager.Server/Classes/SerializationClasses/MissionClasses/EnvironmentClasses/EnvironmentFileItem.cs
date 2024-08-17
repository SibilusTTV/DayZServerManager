using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EnvironmentClasses
{
    [XmlRoot("file")]
    public class EnvironmentFileItem
    {
        [XmlAttribute("path")]
        public string? Path { get; set; }

        [XmlAttribute("usable")]
        public string? Usable { get; set; }
    }
}
