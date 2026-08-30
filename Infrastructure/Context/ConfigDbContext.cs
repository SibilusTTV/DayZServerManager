using Domain.Manager;
using Domain.Scheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context;

public class ConfigDbContext : DbContext
{
    private ILogger<ConfigDbContext> _logger;
    
    public DbSet<Instance> INSTANCES { get; set; }
    public DbSet<SteamCredentials> STEAM_CREDENTIALS { get; set; }
    public DbSet<Mod> MODS { get; set; }
    public DbSet<CustomMessage> MESSAGES { get; set; }
    public DbSet<User> PLAYERS { get; set; }
    public DbSet<ServerPlayer> SERVER_PLAYERS { get; set; }
    public DbSet<SchedulerConfig> SCHEDULER_CONFIGS { get; set; }
    public DbSet<InstanceClientMod> INSTANCE_CLIENT_MODS { get; set; }
    public DbSet<InstanceServerMod> INSTANCE_SERVER_MODS { get; set; }
    public DbSet<Role> ROLES { get; set; }

    public ConfigDbContext(ILogger<ConfigDbContext> logger, DbContextOptions<ConfigDbContext> options) : base(options)
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Instance>()
            .HasMany(config => config.clientMods)
            .WithMany()
            .UsingEntity<InstanceClientMod>(j => j.ToTable("INSTANCE_CLIENT_MODS"));
        
        modelBuilder.Entity<Instance>()
            .HasMany(config => config.serverMods)
            .WithMany()
            .UsingEntity<InstanceServerMod>(j => j.ToTable("INSTANCE_SERVER_MODS"));
        
        modelBuilder.Entity<Instance>()
            .HasMany(config => config.customMessages)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<InstanceServerMod>()
            .HasKey(r => new { r.InstanceId, r.ModId });
        
        modelBuilder.Entity<InstanceClientMod>()
            .HasKey(r => new { r.InstanceId, r.ModId });
        
        modelBuilder.Entity<ServerPlayer>()
            .HasOne(player => player.User)
            .WithMany();
        
        modelBuilder.Entity<ServerPlayer>()
            .HasOne(player => player.Instance)
            .WithMany();
        
        modelBuilder.Entity<ServerPlayer>()
            .HasOne(player => player.Role)
            .WithMany();
    }
}