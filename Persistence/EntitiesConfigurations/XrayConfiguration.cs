namespace MedAI.Persistence.EntitiesConfigurations;

public class XrayConfiguration : IEntityTypeConfiguration<Xray>
{
    public void Configure(EntityTypeBuilder<Xray> builder)
    {
        builder.Property(x => x.AI_Diagnosis)
            .HasMaxLength(255);

        builder.Property(x => x.FinalDiagnosis)
            .HasMaxLength(255);

        builder.Property(x => x.AI_Confidence)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.FinalConfidence)
            .HasColumnType("decimal(5,2)");

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
