namespace DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses
{
    public class FlagsChangesItem
    {
        public string count_in_cargo { get; set; }
        public string count_in_hoarder { get; set; }
        public string count_in_map { get; set; }
        public string count_in_player { get; set; }
        public string crafted { get; set; }
        public string deloot { get; set; }

        public FlagsChangesItem(string count_in_cargo, string count_in_hoarder, string count_in_map, string count_in_player, string crafted, string deloot)
        {
            this.count_in_cargo = count_in_cargo;
            this.count_in_hoarder = count_in_hoarder;
            this.count_in_map = count_in_map;
            this.count_in_player = count_in_player;
            this.crafted = crafted;
            this.deloot = deloot;
        }
    }
}
