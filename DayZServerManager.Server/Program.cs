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

Task task = new Task(() => { Manager.InitiateManager(); });
task.Start();

app.Run("http://0.0.0.0:5172");
