using DayZServerManager.Server.Classes;
using NLog;
using System.Text;
using LogLevel = NLog.LogLevel;

NLog.LogManager.Setup().LoadConfiguration(LogBuilder => {
    LogBuilder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToConsole();
    LogBuilder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(fileName: "manager.log");
});

NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Fully replace Console with logger
Logger.Info("Initializing Manager");
Manager.InitiateManager();
Logger.Info(Manager.STATUS_LISTENING);

app.Run("http://0.0.0.0:" + (Manager.managerConfig != null ? Manager.managerConfig.managerPort : 5172));
