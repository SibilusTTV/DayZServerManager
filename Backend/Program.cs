using Application.IRepository;
using Application.IService;
using Application.Service;
using Domain.Manager;
using Infrastructure.Context;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration.GetValue<int>("Port", 5041);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

Environment.SetEnvironmentVariable("AppendManifestToken_SQLiteProviderManifest",";BinaryGUID=True;");

builder.Services.AddDbContext<ConfigDbContext>(options =>
    options.UseSqlite(
        "Data Source=config.db;", 
        x => x.MigrationsAssembly("Migrations"))
    );

// Serializers
builder.Services.AddScoped<IJsonSerializerRepository, JsonSerializerRepository>();
builder.Services.AddScoped<IInitFileSerializerRepository, InitFileSerializerRepository>();
builder.Services.AddScoped<IXmlSerializerRepository, XmlSerializerRepository>();
builder.Services.AddScoped<IServerConfigSerializerRepository, ServerConfigSerializerRepository>();

// Repositories
builder.Services.AddScoped<IServerRepository, ServerRepository>();
builder.Services.AddScoped<IServerConfigRepository, ServerConfigRepository>();
builder.Services.AddScoped<IRarityRepository, RarityRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<IManagerRepository, ManagerRepository>();
builder.Services.AddScoped<ISchedulerRepository, SchedulerRepository>();

// Database Repos
builder.Services.AddScoped<IModRepository, ModRepository>();
builder.Services.AddScoped<IInstanceRepository, InstanceRepository>();
builder.Services.AddScoped<ISteamCmdRepository, SteamCmdRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

// Services
builder.Services.AddScoped<IRconService, RconService>();
builder.Services.AddScoped<IRestartUpdaterService, RestartUpdaterService>();
builder.Services.AddScoped<ISchedulerService, SchedulerService>();

builder.Services.AddScoped<IServerConfigService, ServerConfigService>();

builder.Services.AddScoped<IRarityService, RarityService>();
builder.Services.AddScoped<IMissionService, MissionService>();

builder.Services.AddScoped<IModService, ModService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();

// Singletons
builder.Services.AddSingleton<ISteamCmdService, SteamCmdService>();
builder.Services.AddSingleton<IServerFactory, ServerFactory>();
builder.Services.AddSingleton<IInstanceService, InstanceService>();

// Hosted Services
builder.Services.AddHostedService<HostedService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
    context.Database.Migrate();
    
    Console.WriteLine("Configuration database ready!");
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("Frontend/browser/index.html");

app.Run("http://0.0.0.0:" + port);