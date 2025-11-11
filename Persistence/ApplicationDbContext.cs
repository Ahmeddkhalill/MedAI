using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MedAI.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options ) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Xray> Xrays { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
