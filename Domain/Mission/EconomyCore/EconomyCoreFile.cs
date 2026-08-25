using System.Xml.Serialization;

namespace Domain.Mission.EconomyCore;

[XmlRoot("economycore")]
public class EconomyCoreFile
{
    [XmlElement("classes")]
    public ClassesItem classes;
    [XmlElement("defaults")]
    public DefaultsItem defaults;
    [XmlElement("ce")]
    public List<CeItem> ceItems;
}
