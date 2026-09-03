using Application.IService;
using Domain.Scheduler;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SchedulerController : ControllerBase
{
    private readonly ILogger<InstanceController> _logger;
    private readonly ISchedulerService _schedulerService;

    public SchedulerController(ILogger<InstanceController> logger, ISchedulerService schedulerService)
    {
        _logger = logger;
        _schedulerService = schedulerService;
    }
    
    [HttpGet]
    public SchedulerConfig? GetSchedulerConfig(int instanceId)
    {
        return _schedulerService.Get(instanceId);
    }

    [HttpPost]
    public void CreateEditSchedulerConfig(SchedulerConfig schedulerConfig)
    {
        _schedulerService.CreateEdit(schedulerConfig);
    }
}