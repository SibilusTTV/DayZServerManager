using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses
{

    public class RarityFile
    {
        public int m_Version { get; set; }
        public int PoorItemRequirement { get; set; }
        public int CommonItemRequirement { get; set; }
        public int UncommonItemRequirement { get; set; }
        public int RareItemRequirement { get; set; }
        public int EpicItemRequirement { get; set; }
        public int LegendaryItemRequirement { get; set; }
        public int MythicItemRequirement { get; set; }
        public int ExoticItemRequirement { get; set; }
        public int ShowHardlineHUD { get; set; }
        public int UseReputation { get; set; }
        public int UseFactionReputation { get; set; }
        public int EnableFactionPersistence { get; set; }
        public int EnableItemRarity { get; set; }
        public int UseItemRarityOnInventoryIcons { get; set; }
        public int UseItemRarityForMarketPurchase { get; set; }
        public int UseItemRarityForMarketSell { get; set; }
        public int MaxReputation { get; set; }
        public int ReputationLossOnDeath { get; set; }
        public int DefaultItemRarity { get; set; }
        public int ItemRarityParentSearch { get; set; }
        public Dictionary<string, int> EntityReputation { get; set; }
        public Dictionary<string, int> ItemRarity { get; set; }
    }
}
