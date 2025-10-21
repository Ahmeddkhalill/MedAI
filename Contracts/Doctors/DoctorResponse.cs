using System.Text.Json.Serialization;

namespace MedAI.Contracts.Doctors;

public record DoctorResponse
(
    int Id,
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] Degree Degree
);
