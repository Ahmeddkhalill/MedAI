using MedAI.Contracts.Doctors;

namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorsController(
    IDoctorService doctorService,
    IScheduleService doctorAvailableTimeService,
    IBookingService bookingService) : ControllerBase
{
    private readonly IDoctorService _doctorService = doctorService;
    private readonly IScheduleService _doctorAvailableTimeService = doctorAvailableTimeService;
    private readonly IBookingService _bookingService = bookingService;

    [HttpGet("dashboard")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetDoctorDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllDoctors([FromQuery] RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetAllAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDoctorById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddDoctor([FromBody] AddDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.AddDoctorAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await _doctorService.UpdateDoctorAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPut("complete-profile")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> CompleteProfile([FromForm] CompleteProfileRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.CompleteProfileAsync(request, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDoctor(int id, CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{doctorId}/schedule")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDoctorAvailableTimes(int doctorId, CancellationToken cancellationToken = default)
    {
        var result = await _doctorAvailableTimeService.GetByDoctorAsync(doctorId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("my-appointments")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] RequestFilters filters, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetDoctorAppointmentsAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("me")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetMyProfileAsync(cancellationToken); 
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}