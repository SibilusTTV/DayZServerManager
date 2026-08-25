using System.Xml.Serialization;

namespace Domain.Mission.EconomyCore;
    
public class CeItem
{
    [XmlAttribute("folder")]
    public string folder;
    [XmlElement("file")]
    public List<FileItem> fileItems;
}
