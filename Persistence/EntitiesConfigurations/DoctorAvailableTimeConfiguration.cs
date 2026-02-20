namespace MedAI.Persistence.EntitiesConfigurations;

public class DoctorAvailableTimeConfiguration : IEntityTypeConfiguration<DoctorAvailableTime>
{
    public void Configure(EntityTypeBuilder<DoctorAvailableTime> builder)
    {
        builder.HasOne(x => x.Doctor)
        .WithMany(d => d.AvailableTimes)
        .HasForeignKey(x => x.DoctorId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ConsultationFee)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.BookedCount)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.DoctorId, x.Date });
    }
}