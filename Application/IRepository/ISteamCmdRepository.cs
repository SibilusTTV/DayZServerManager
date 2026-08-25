using Domain.Manager;

namespace Application.IRepository;

public interface ISteamCmdRepository
{
    public SteamCredentials? GetCredentials();
    public string GetSteamUsername();
    public string GetSteamPassword();
    public void SaveCredentials(SteamCredentials credentials);
}