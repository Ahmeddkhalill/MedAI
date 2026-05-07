namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController(IAdminService adminService) : ControllerBase
{
    private readonly IAdminService _adminService = adminService;

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
