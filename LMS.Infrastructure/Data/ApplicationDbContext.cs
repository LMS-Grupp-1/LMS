using Domain.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
	public DbSet<Course> Courses => Set<Course>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		// Must run first, it configures the Identity tables
		base.OnModelCreating(builder);

		// Picks up all IEntityTypeConfiguration classes in this assembly
		builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
	}
}