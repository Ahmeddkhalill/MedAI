namespace MedAI.Entities;

public class Booking
{
    public int Id { get; set; }
    public bool IsCancelled { get; set; } = false;
    public int DoctorAvailableTimeId { get; set; }
    public DoctorAvailableTime DoctorAvailableTime { get; set; } = default!;

    public string PatientId { get; set; } = default!;
    public ApplicationUser Patient { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}