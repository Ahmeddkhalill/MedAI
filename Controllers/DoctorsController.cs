using MedAI.Contracts.Common;
using MedAI.Contracts.Doctors;

namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DoctorsController(IDoctorService doctorService) : ControllerBase
{
    private readonly IDoctorService _doctorService = doctorService;

    [HttpGet("")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllDoctors([FromQuery] RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetAllAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value): result.ToProblem();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDoctorByIdAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    public async Task<IActionResult> AddDoctorAsync([FromBody] AddDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.AddDoctorAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDoctorAsync([FromRoute] int id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.UpdateDoctorAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctorAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
