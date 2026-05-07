namespace MedAI.Errors;

public record DoctorErrors
{
    public static readonly Error DuplicatedEmail =
        new("Doctor.DuplicatedEmail", "Email already exists", StatusCodes.Status409Conflict);

    public static readonly Error NotFound =
        new("Doctor.NotFound", "Doctor not found", StatusCodes.Status404NotFound);
   
    public static readonly Error Unauthorized =
            new("Doctor.Unauthorized", "Unauthorized access", StatusCodes.Status401Unauthorized);
}
