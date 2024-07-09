using DayZServerManager.Server.Classes.SerializationClasses.ManagerConfigClasses;
using DayZServerManager;
using DayZServerManager.Server.Classes;
using DayZServerManager.Server.Classes.Helpers;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.RarityClasses;
using DayZServerManager.Server.Classes.SerializationClasses.MissionClasses.TypesClasses;
using DayZServerManager.Server.Classes.SerializationClasses.BecClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ProfileClasses.NotificationSchedulerClasses;
using DayZServerManager.Server.Classes.SerializationClasses.ServerConfigClasses;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

Manager.LoadManagerConfig();

Manager.LoadServerConfig();
Manager.AdjustServerConfig();
Manager.SaveManagerConfig();

if (Manager.managerConfig.autoStartServer)
{
    Manager.StartServer();
}

app.Run();

void OnProcessExit(object? sender, EventArgs? e)
{
    Manager.KillServerProcesses();
}

void WriteToConsole(string message)
{
    System.Console.WriteLine(Environment.NewLine + DateTime.Now.ToString("[HH:mm:ss]") + message);
}
