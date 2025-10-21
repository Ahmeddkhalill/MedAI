namespace MedAI.Errors;

public static class DoctorErrors
{
    public static readonly Error DuplicatedEmail =
        new("Doctor.DuplicatedEmail", "Email already exists", StatusCodes.Status409Conflict);

    public static readonly Error NotFound =
        new("Doctor.NotFound", "Doctor not found", StatusCodes.Status404NotFound);
}
