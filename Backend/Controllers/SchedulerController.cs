using Application.IService;
using Domain.Scheduler;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SchedulerController
{
    private readonly ILogger<SchedulerController> _logger;
    private readonly ISchedulerService _schedulerService;

    public SchedulerController(ILogger<SchedulerController> logger, ISchedulerService schedulerService)
    {
        _logger = logger;
        _schedulerService = schedulerService;
    }
}