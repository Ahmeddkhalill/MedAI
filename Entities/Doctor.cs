namespace MedAI.Entities;

public class Doctor
{
    public int Id { get; set; }
    public Degree Degree { get; set; }

    public string UserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public ICollection<DoctorAvailableTime> AvailableTimes { get; set; } = new List<DoctorAvailableTime>();
}
public enum Degree
{
    GeneralPractitioner = 0,
    Specialist = 1,
    Consultant = 2,
    Professor = 3
}