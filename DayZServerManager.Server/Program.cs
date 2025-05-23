
using DayZServerManager.Server.Classes;
using NLog;
using NLog.Web;
using NLog.Extensions.Logging;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();

builder.Host.UseNLog();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var enc1252 = Encoding.GetEncoding(1252);

AppDomain.CurrentDomain.ProcessExit += new EventHandler((s, e) => { Manager.KillServerOnClose(); });

//NLog.LogManager.Setup().LoadConfiguration(LogBuilder => {
//    LogBuilder.ForLogger().FilterMinLevel(NLog.LogLevel.Debug).WriteToConsole();
//    LogBuilder.ForLogger().FilterMinLevel(NLog.LogLevel.Info).WriteToFile(
//        fileName: Manager.LOGS_PATH + "/manager.log",
//        layout: "${longdate}|${callsite}|${message}|${exception}",
//        archiveAboveSize: 1048576,
//        maxArchiveDays: Manager.managerConfig.maxKeepTime
//    );
//});

NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

// Fully replace Console with logger
Logger.Info("Initializing Manager");
Manager.InitializeManager();
Logger.Info(Manager.STATUS_LISTENING);

app.Run("http://0.0.0.0:" + (Manager.managerConfig != null ? Manager.managerConfig.managerPort : 5172));
