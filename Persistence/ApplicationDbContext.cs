using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MedAI.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options ) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
