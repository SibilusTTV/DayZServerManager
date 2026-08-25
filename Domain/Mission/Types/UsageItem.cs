using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Domain.Mission.Types;

[XmlRoot("category")]
public class UsageItem
{
    [XmlAttribute("name")]
    public string name;
}
