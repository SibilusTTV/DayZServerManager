using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using Application.IRepository;
using Application.IService;
using Domain.Constants;
using Domain.Manager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class SteamCmdService : ISteamCmdService
{
    private readonly ILogger<SteamCmdService> _logger;
    private readonly IServiceScope _serverScope;
    
    public SteamInformation SteamInformation {get; private set;}
    private readonly HttpClient _client;
    
    private Process? _steamCmdProcess;
    
    private Timer? _updateTimer;

    public SteamCmdService(ILogger<SteamCmdService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _steamCmdProcess = null;
        _serverScope = scopeFactory.CreateScope();
        SteamInformation = new SteamInformation();
        _client = new HttpClient();
    }

    public void StartTimer()
    {
        _updateTimer = new Timer(UpdateLoop, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }
    
    public void Stop()
    {
        try
        {
            _updateTimer?.Dispose();
            _updateTimer = null;
            if (_steamCmdProcess is { HasExited: false } || SteamInformation.steamCmdStatus != Statuses.NotRunning || SteamInformation.steamCmdStatus != Statuses.SteamGuard)
            {
                _steamCmdProcess?.WaitForExit();
            }
            _steamCmdProcess?.Kill();
            _steamCmdProcess?.Dispose();
            _steamCmdProcess = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when killing steamCmd");
            _updateTimer = null;
            _steamCmdProcess = null;
        }
        
        SteamInformation.steamCmdStatus = Statuses.NotRunning;
        SteamInformation.SteamUpdateLoop = Statuses.NotRunning;
    }
    
    public void Dispose()
    {
        _serverScope?.Dispose();
    }

    private void UpdateLoop(object? state)
    {
        SteamInformation.SteamUpdateLoop = Statuses.Running;
        UpdateServer();
        UpdateMods();
        SteamInformation.SteamUpdateLoop = Statuses.NotRunning;
    }

    public SteamInformation GetSteamInformation()
    {
        return SteamInformation;
    }

    public bool CheckSteamCmd()
    {
        return SteamInformation.SteamUpdateLoop != Statuses.NotRunning;
    }

    public bool CheckUpdateLoop()
    {
        return _updateTimer != null;
    }

    public bool WriteSteamGuard(string code)
    {
        try
        {
            if (SteamInformation.steamCmdStatus != Statuses.NotRunning)
            {
                if (!_steamCmdProcess?.HasExited ?? false)
                {
                    _steamCmdProcess.StandardInput.WriteLine(code);
                    _logger.LogInformation("Steam guard written");
                    return true;
                }
            }
            else
            {
                SteamInformation.steamCmdStatus = Statuses.NotRunning;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when writing SteamGuard");
            return false;
        }
    }

    public void WaitForSteamCmd()
    {
        while ((SteamInformation.steamCmdStatus != Statuses.NotRunning &&
                SteamInformation.steamCmdStatus != Statuses.ModsUpdated &&
                SteamInformation.steamCmdStatus != Statuses.ServerUpdated) || 
               SteamInformation.SteamUpdateLoop != Statuses.NotRunning)
        {
            _steamCmdProcess?.WaitForExit();
            Thread.Sleep(1000);
        }
    }

    public SteamCredentials GetSteamCredentials()
    {
        var repository = _serverScope.ServiceProvider.GetService<ISteamCmdRepository>();
        return repository?.GetCredentials() ?? new SteamCredentials();
    }

    public string GetSteamUsername()
    {
        var repository = _serverScope.ServiceProvider.GetService<ISteamCmdRepository>();
        return repository?.GetSteamUsername() ?? "";
    }

    public string GetSteamPassword()
    {
        var repository = _serverScope.ServiceProvider.GetService<ISteamCmdRepository>();
        return repository?.GetSteamPassword() ?? "";
    }

    public void SaveSteamCredentials(SteamCredentials credentials)
    {
        var repository = _serverScope.ServiceProvider.GetService<ISteamCmdRepository>();
        repository?.SaveCredentials(credentials);
    }
    
    private void UpdateServer()
    {
        UpdateSteamCmd();

        try
        {
            var steamUsername = GetSteamUsername();
            var serverUpdateArguments = $"\"+force_install_dir {Path.Combine("..", Folders.DeployFolderName)}\" \"+login {steamUsername}\" \"+app_update {SteamCmd.DayZServerBranch}\" -validate +quit";
            _logger.LogInformation("Updating the DayZ Server");
            StartSteamCmd(serverUpdateArguments);
            SteamInformation.steamCmdStatus = Statuses.ServerUpdated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when updating server");
            SteamInformation.steamCmdStatus = Statuses.Error;
        }
    }
    
    private void UpdateMods()
    {
        try
        {
            var mods = GetMods();
            
            if (mods.Count <= 0) return;
            
            var steamUsername = GetSteamUsername();
            var modUpdateArguments = string.Empty;
            foreach (var mod in mods)
            {
                modUpdateArguments += $" +workshop_download_item {SteamCmd.DayZGameBranch} {mod.workshopID.ToString()}";
            }
            var arguments = $"\"+force_install_dir {Path.Combine("..", Folders.ModsFolderName)}\" \"+login {steamUsername}\" {modUpdateArguments} +quit";

            StartSteamCmd(arguments);

            _logger.LogInformation($"All mods were downloaded");

            _logger.LogInformation(Statuses.ModsUpdated);
            SteamInformation.steamCmdStatus = Statuses.ModsUpdated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when updating mods");
            SteamInformation.steamCmdStatus = Statuses.Error;
        }
    }

    private void UpdateSteamCmd()
    {
        try
        {
            if (!Directory.Exists(Folders.SteamCmdFolderName))
            {
                Directory.CreateDirectory(Folders.SteamCmdFolderName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when creating steamCmd path");
        }

        try
        {
            if (!File.Exists(Path.Combine(Folders.SteamCmdFolderName, Files.SteamCmdExecutableFileName)))
            {
                DownloadAndExtractSteamCmd(Files.SteamCmdZipName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when downloading and extracting steamCmd");
            SteamInformation.steamCmdStatus = Statuses.Error;
        }
    }
    
    private void DownloadAndExtractSteamCmd(string zipName)
    {
        try
        {
            var response = _client.GetAsync(Urls.SteamCmdDownloadUrl + Files.SteamCmdZipName).Result;

            if (!response.IsSuccessStatusCode) return;
            
            var content = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(Path.Combine(Folders.SteamCmdFolderName, zipName), content);
            if (OperatingSystem.IsWindows())
            {
                ZipFile.ExtractToDirectory(Path.Combine(Folders.SteamCmdFolderName, zipName), Folders.SteamCmdFolderName);
            }
            else
            {
                using (var compressedFileStream = File.Open(Path.Combine(Folders.SteamCmdFolderName, zipName), FileMode.Open))
                {
                    using (var outputFileStream = File.Create(Path.Combine(Folders.SteamCmdFolderName, Files.SteamCmdTarFileName)))
                    {
                        using (var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                        {
                            decompressor.CopyTo(outputFileStream);
                        }
                    }
                }
                if (File.Exists(Path.Combine(Folders.SteamCmdFolderName, Files.SteamCmdTarFileName)))
                {
                    TarFile.ExtractToDirectory(Path.Combine(Folders.SteamCmdFolderName, Files.SteamCmdTarFileName), Folders.SteamCmdFolderName, true);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when downloading and extracting steamCmd");
            SteamInformation.steamCmdStatus = Statuses.Error;
        }
    }
    
    private void StartSteamCmd(string serverUpdateArguments)
    {
        try
        {
            _steamCmdProcess = new Process();
            lock (_steamCmdProcess)
            {
                _steamCmdProcess.StartInfo.UseShellExecute = false;
                _steamCmdProcess.StartInfo.Arguments = serverUpdateArguments;
                _steamCmdProcess.StartInfo.RedirectStandardError = true;
                _steamCmdProcess.StartInfo.RedirectStandardInput = true;
                _steamCmdProcess.StartInfo.RedirectStandardOutput = true;
                _logger.LogInformation(Path.Combine(Folders.SteamCmdFolderName, Files.SteamCmdExecutableFileName) + " " + serverUpdateArguments);
                _steamCmdProcess.StartInfo.FileName = Path.Combine(Folders.SteamCmdFolderName, Files.SteamCmdExecutableFileName);

                if (OperatingSystem.IsWindows())
                {
                    if (!Directory.Exists(Path.Combine(Folders.SteamCmdFolderName, Folders.SteamCmdLogsFolderName)))
                    {
                        Directory.CreateDirectory(Path.Combine(Folders.SteamCmdFolderName, Folders.SteamCmdLogsFolderName));
                    }
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(Folders.SteamCmdFolderName, Folders.SteamCmdLinux32FolderName)))
                    {
                        Directory.CreateDirectory(Path.Combine(Folders.SteamCmdFolderName, Folders.SteamCmdLinux32FolderName));
                    }

                    if (!Directory.Exists(Path.Combine(Folders.SteamCmdFolderName, Folders.SteamCmdLinux32FolderName, Folders.SteamCmdLogsFolderName)))
                    {
                        Directory.CreateDirectory(Path.Combine(Folders.SteamCmdFolderName, Folders.SteamCmdLinux32FolderName, Folders.SteamCmdLogsFolderName));
                    }
                }

                if (!File.Exists(Path.Combine(Folders.SteamcmdConsoleLogFolderPath, Files.SteamCmdConsoleLogFileName)))
                {
                    using (var fs = File.Create(Path.Combine(Folders.SteamcmdConsoleLogFolderPath, Files.SteamCmdConsoleLogFileName)))
                    {

                    }
                }

                _steamCmdProcess.Start();

                SteamInformation.steamCmdStatus = Statuses.Running;

                using (var fs = new FileStream(Path.Combine(Folders.SteamcmdConsoleLogFolderPath, Files.SteamCmdConsoleLogFileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.Default))
                    {
                        var hasExited = false;
                        while (!_steamCmdProcess.HasExited && !hasExited)
                        {
                            var standardOutput = sr.ReadToEnd();

                            if (string.IsNullOrEmpty(standardOutput)) continue;
                            
                            _logger.LogInformation(standardOutput);
                            if (standardOutput.Contains("password:"))
                            {
                                var steamPassword = GetSteamPassword();
                                _steamCmdProcess.StandardInput.WriteLine(steamPassword);
                                _logger.LogInformation(Statuses.CachedCredentials);
                                SteamInformation.steamCmdStatus = Statuses.CachedCredentials;
                            }
                            else if (standardOutput.Contains("Steam Guard code:"))
                            {
                                _logger.LogInformation(Statuses.SteamGuard);
                                SteamInformation.steamCmdStatus = Statuses.SteamGuard;
                            }
                            else if (standardOutput.Contains("client config"))
                            {
                                SteamInformation.steamCmdStatus = Statuses.ClientConfig;
                            }
                            else if (standardOutput.Contains("Unloading Steam API"))
                            {
                                _steamCmdProcess.WaitForExit();
                                hasExited = true;
                            }
                        }

                        _logger.LogInformation(sr.ReadToEnd());

                    }
                }

                if (_steamCmdProcess != null)
                {
                    _steamCmdProcess.Kill();
                    _steamCmdProcess = null;
                }

                using (var writer = new StreamWriter(Path.Combine(Folders.SteamcmdConsoleLogFolderPath, Files.SteamCmdConsoleLogFileName), false))
                {
                    writer.Write(string.Empty);
                }

                SteamInformation.steamCmdStatus = Statuses.NotRunning;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when starting steam");
            SteamInformation.steamCmdStatus = Statuses.Error;
        }
    }

    private List<Mod> GetMods()
    {
        var repository = _serverScope.ServiceProvider.GetService<IModRepository>();
        return repository?.GetMods() ?? [];
    }
}