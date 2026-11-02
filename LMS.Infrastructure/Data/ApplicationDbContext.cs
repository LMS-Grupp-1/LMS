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
	public DbSet<Assignment> Assignments => Set<Assignment>();
	public DbSet<Submission> Submissions => Set<Submission>();
	public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<SharedDocument> SharedDocuments => Set<SharedDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // This automatically registers every Configuration that implement IEntityTypeConfiguration<T> in ApplicationDbContext
        // So do not need to register every new configuration like CourseConfiguration/ActivityConfiguration.
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext)
            .Assembly);
    }
}