using Application.IService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class HostedService : IHostedService
{
    private readonly ILogger<HostedService> _logger;
    private readonly IInstanceService _instanceService;

    public HostedService(ILogger<HostedService> logger, IInstanceService instanceService)
    {
        _logger = logger;
        _instanceService = instanceService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _instanceService.Initialize();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _instanceService.Dispose();
        return Task.CompletedTask;
    }
}