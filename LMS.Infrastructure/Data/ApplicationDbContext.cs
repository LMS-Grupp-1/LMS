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
}