namespace MedAI.Contracts.Doctors;

public class UpdateDoctorRequest
{
    public string Email { get; set; } = default!;
    public string Degree { get; set; } = default!;
    public string Speciality { get; set; } = default!;
    public IFormFile? Image { get; set; }
}