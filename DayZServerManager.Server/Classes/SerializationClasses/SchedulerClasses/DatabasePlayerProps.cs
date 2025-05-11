
namespace DayZServerManager.Server.Classes.SerializationClasses.SchedulerClasses
{
    public class DatabasePlayerProps
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public string Uid { get; set; }
        public bool IsVerified { get; set; }
        public string IP { get; set; }
        public bool IsBanned { get; set; }
        public string BannedReasons { get; set; }
        public bool IsWhitelisted { get; set; }

        public DatabasePlayerProps(string name, string guid, string uid, bool isVerified, string ip, bool isBanned, string bannedReasons, bool isWhitelisted)
        {
            Name = name;
            Guid = guid;
            Uid = uid;
            IsVerified = isVerified;
            IP = ip;
            IsBanned = isBanned;
            BannedReasons = bannedReasons;
            IsWhitelisted = isWhitelisted;
        }
    }
}
