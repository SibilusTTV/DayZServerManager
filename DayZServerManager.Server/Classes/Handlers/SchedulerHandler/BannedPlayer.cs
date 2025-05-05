namespace DayZServerManager.Server.Classes.Handlers.SchedulerHandler
{
    public class BannedPlayer
    {
        public int BanId { get; set; }
        public string Guid { get; set; }
        public int RemainingTime { get; set; }
        public string Reason { get; set; }

        public BannedPlayer(int banId, string guid, int remainingTime, string reason)
        {
            BanId = banId;
            Guid = guid;
            RemainingTime = remainingTime;
            Reason = reason;
        }
    }
}
