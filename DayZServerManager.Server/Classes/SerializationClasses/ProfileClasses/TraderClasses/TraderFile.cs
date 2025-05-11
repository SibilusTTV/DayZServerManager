using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.TraderClasses
{
    internal class TraderFile
    {
        public int m_Version { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int IsExchange { get; set; }
        public float InitStockPercent { get; set; }
        public List<TraderItem> Items { get; set; }

        public TraderFile(int version, string displayName, string icon, string color, int isExchange, float initStockPercent)
        {
            m_Version = version;
            DisplayName = displayName;
            Icon = icon;
            Color = color;
            IsExchange = isExchange;
            InitStockPercent = initStockPercent;
            Items = new List<TraderItem>();
        }
    }
}
