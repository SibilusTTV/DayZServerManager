using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EnvironmentClasses
{
    [XmlRoot("item")]
    public class ItemElement
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("val")]
        public string? Val {  get; set; }
    }
}
