namespace MedAI.Entities;

public class Doctor
{
    public int Id { get; set; }
    public string Degree { get; set; } = default!;
    public string Speciality { get; set; } = default!;
    public string? ImageUrl { get; set; }

    public string UserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public ICollection<DoctorAvailableTime> AvailableTimes { get; set; } = [];
}