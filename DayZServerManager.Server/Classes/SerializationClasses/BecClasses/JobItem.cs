using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.BecClasses
{
    [XmlRoot("Job")]
    public class JobItem
    {
        [XmlAttribute("id")]
        public int id;
        [XmlElement("day")]
        public string days;
        [XmlElement("start")]
        public string start;
        [XmlElement("runtime")]
        public string runtime;
        [XmlElement("loop")]
        public int loop;
        [XmlElement("cmd")]
        public string cmd;

        public JobItem() { }

        public JobItem(int id, string days, string start, string runtime, int loop, string cmd)
        {
            this.id = id;
            this.days = days;
            this.start = start;
            this.runtime = runtime;
            this.loop = loop;
            this.cmd = cmd;
        }
    }
}
