using Microsoft.AspNetCore.Identity;

namespace MedAI.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Doctor? Doctor { get; set; }
}
