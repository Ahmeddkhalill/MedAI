namespace MedAI.Contracts.Doctors;

public class AddDoctorRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Degree { get; set; } = default!;
    public string Speciality { get; set; } = default!;
    public IFormFile? Image { get; set; }
}