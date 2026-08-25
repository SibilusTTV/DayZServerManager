using Domain.Scheduler;

namespace Application.IRepository;

public interface IPlayerRepository
{
    public List<Player> GetAllPlayers();
    public Player? GetPlayer(Guid id);
    public List<Player> GetPlayersByName(string name);
    public void CreateEditPlayer(Player player);
    public List<ServerPlayer> GetServerPlayersForInstance(Guid instanceId);
    public ServerPlayer? GetServerPlayer(Guid playerId);
    public List<ServerPlayerInformation> GetServerPlayerInformationForInstance(Guid instanceId);
    public void CreateEditServerPlayer(ServerPlayer player);
    public void ClearBans();
    public ServerPlayer? GetBannedServerPlayer(int banId, Guid guid, string reason);
    public void CreateNewBan(int banId, Guid guid, int remainingTime, string reason);
    public void RemoveBan(Guid id);
    public void UpdateRemainingTime(Guid id, int remainingTime);
    public List<string> GetWhitelistedPlayerNames(Guid instanceId);
    public void WhitelistPlayer(Guid serverPlayerId);
    public void UnWhitelistPlayer(Guid serverPlayerId);
}