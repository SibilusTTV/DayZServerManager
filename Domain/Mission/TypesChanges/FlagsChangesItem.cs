using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Domain.Mission.TypesChanges;

public class FlagsChangesItem
{
    public string count_in_cargo { get; set; }
    public string count_in_hoarder { get; set; }
    public string count_in_map { get; set; }
    public string count_in_player { get; set; }
    public string crafted { get; set; }
    public string deloot { get; set; }

    public FlagsChangesItem()
    {
        
    }

    public FlagsChangesItem(string countInCargo, string countInHoarder, string countInMap, string countInPlayer, string crafted, string deloot)
    {
        count_in_cargo = countInCargo;
        count_in_hoarder = countInHoarder;
        count_in_map = countInMap;
        count_in_player = countInPlayer;
        this.crafted = crafted;
        this.deloot = deloot;
    }
}
