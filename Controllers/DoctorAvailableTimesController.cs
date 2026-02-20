using MedAI.Contracts.DoctorAvailableTime;

namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Doctor")]
public class DoctorAvailableTimesController(IDoctorAvailableTimeService doctorAvailableTimeService) : ControllerBase
{
    private readonly IDoctorAvailableTimeService _doctorAvailableTimeService = doctorAvailableTimeService;

    [HttpPost]
    public async Task<IActionResult> Add(AddDoctorAvailableTimeRequest request, CancellationToken cancellationToken)
    {
        var result = await _doctorAvailableTimeService.AddAsync(request, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
