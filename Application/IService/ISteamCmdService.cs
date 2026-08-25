using System.Diagnostics;
using Domain.Manager;

namespace Application.IService;

public interface ISteamCmdService
{
    public SteamInformation SteamInformation {get; }
    public void StartTimer();
    public void Stop();
    public void Dispose();
    public SteamInformation GetSteamInformation();
    public bool CheckSteamCmd();
    public bool CheckUpdateLoop();
    public bool WriteSteamGuard(string code);
    public void WaitForSteamCmd();
    public SteamCredentials GetSteamCredentials();
    public string GetSteamUsername();
    public string GetSteamPassword();
    public void SaveSteamCredentials(SteamCredentials credentials);
}