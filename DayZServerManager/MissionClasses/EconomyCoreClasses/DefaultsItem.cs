﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.MissionClasses.EconomyCoreClasses
{
    [XmlRoot("defaults")]
    public class DefaultsItem
    {
        [XmlElement("default")]
        public List<DefaultItem> defaults;
    }
}
