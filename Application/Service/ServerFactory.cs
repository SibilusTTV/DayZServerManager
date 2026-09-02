using Application.IService;
using Domain.Manager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class ServerFactory : IServerFactory
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISteamCmdService _steamCmdService;
    private readonly ILoggerFactory _loggerFactory;
    
    public ServerFactory(
        IServiceScopeFactory scopeFactory, 
        IServiceProvider serviceProvider,
        ISteamCmdService steamCmdService,
        ILoggerFactory loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _serviceProvider = serviceProvider;
        _steamCmdService = steamCmdService;
        _loggerFactory = loggerFactory;
    }
    
    public IServerInstance CreateServerAsync(int id)
    {
        var server = new ServerInstance(_loggerFactory.CreateLogger<ServerInstance>(), id, _scopeFactory, _steamCmdService);
        
        // Optionale scoped Services pro Server initialisieren
        using var scope = _scopeFactory.CreateScope();
        // var serverConfig = scope.ServiceProvider.GetRequiredService<IServerConfiguration>();
        // Konfiguration laden und anwenden
        
        return server;
    }
}