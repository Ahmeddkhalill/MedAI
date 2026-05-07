namespace MedAI.Persistence.EntitiesConfigurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {

        builder.HasOne(d => d.ApplicationUser)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.Speciality)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Degree)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.IsAccountCompleted)
            .HasDefaultValue(false);
    }
}
