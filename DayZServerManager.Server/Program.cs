using DayZServerManager.Server.Classes;

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

AppDomain.CurrentDomain.ProcessExit += new EventHandler((s, e) => { Manager.KillServerOnClose(); });

Manager.WriteToConsole("Initializing Manager");
Manager.InitiateManager();
Manager.WriteToConsole("Listening");

app.Run("http://0.0.0.0:" + (Manager.managerConfig != null ? Manager.managerConfig.managerPort : 5172));
