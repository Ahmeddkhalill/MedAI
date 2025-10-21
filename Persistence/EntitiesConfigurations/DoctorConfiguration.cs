namespace MedAI.Persistence.EntitiesConfigurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(d => d.Degree).IsRequired();

        builder.HasOne(d => d.ApplicationUser)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
