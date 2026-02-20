using MedAI.Contracts.Doctors;

namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class DoctorsController(IDoctorService doctorService, IDoctorAvailableTimeService doctorAvailableTimeService) : ControllerBase
{
    private readonly IDoctorService _doctorService = doctorService;
    private readonly IDoctorAvailableTimeService _doctorAvailableTimeService = doctorAvailableTimeService;

    [HttpGet("")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllDoctors([FromQuery] RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetAllAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value): result.ToProblem();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDoctorByIdAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddDoctorAsync([FromBody] AddDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.AddDoctorAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDoctorAsync([FromRoute] int id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.UpdateDoctorAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDoctorAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{doctorId}/available-times")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetDoctorAvailableTimesAsync([FromRoute] int doctorId, CancellationToken cancellationToken = default)
    {
        var result = await _doctorAvailableTimeService.GetByDoctorAsync(doctorId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
