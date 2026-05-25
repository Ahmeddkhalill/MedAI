using MedAI.Contracts.Users;

namespace MedAI.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class AccountController(IUserService userService, IBookingService bookingService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IBookingService _bookingService = bookingService;

    [HttpGet("")]
    public async Task<IActionResult> Info()
    {
        var result = await _userService.GetProfileAsync(User.GetUserId()!);
        return Ok(result.Value);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _userService.ChangePasswordAsync(User.GetUserId()!, request);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("info")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
         await _userService.UpdateProfileAsync(User.GetUserId()!, request);
         return NoContent();
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetPatientDashboard(CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetPatientDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
