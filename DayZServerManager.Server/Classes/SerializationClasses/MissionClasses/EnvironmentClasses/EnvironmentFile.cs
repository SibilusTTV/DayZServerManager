using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.EnvironmentClasses
{
    [XmlRoot("env")]
    public class EnvironmentFile
    {
        [XmlElement("territories")]
        public TerritoriesItem? Territories {  get; set; }
    }
}
