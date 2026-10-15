using Domain.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.NativeInterop;
using Module = Domain.Models.Entities.Module;

namespace LMS.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses { get; set; }

    public DbSet<Module> Modules => Set<Module>();
    
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext)
            .Assembly);

        builder.ApplyConfiguration(
            new Configurations.ActivityConfiguration());
    }
}