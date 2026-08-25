using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Domain.Mission.Globals;

[XmlRoot("variables")]
public class GlobalsFile
{
    [XmlElement("var")]
    public VarItem[] varItems;
}
