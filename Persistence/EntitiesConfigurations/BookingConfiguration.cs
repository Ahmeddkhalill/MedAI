namespace MedAI.Persistence.EntitiesConfigurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasOne(x => x.DoctorAvailableTime)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.DoctorAvailableTimeId);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId);

        builder.HasIndex(x => new { x.DoctorAvailableTimeId, x.PatientId })
            .IsUnique();
    }
}
