namespace MedAI.Entities;

public class Xray
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? AI_Diagnosis { get; set; }
    public decimal? AI_Confidence { get; set; }
    public string? DoctorNotes { get; set; }
    public string? FinalDiagnosis { get; set; }
    public decimal? FinalConfidence { get; set; }
    public bool IsRevised { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }

    public string PatientId { get; set; } = null!;
    public ApplicationUser Patient { get; set; } = null!;

    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
}
