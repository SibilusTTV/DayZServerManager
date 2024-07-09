using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.TraderClasses
{
    internal class TraderItem
    {
        public string ClassName { get; set; }
        public int MaxPriceThreshold { get; set; }
        public int MinPriceThreshold { get; set; }
        public float SellPricePercent { get; set; }
        public int MaxStockThreshold { get; set; }
        public int MinStockThreshold { get; set; }
        public float QuantityPercent { get; set; }
        public string[] SpawnAttachments { get; set; }
        public string[] Variants { get; set; }
    }
}
