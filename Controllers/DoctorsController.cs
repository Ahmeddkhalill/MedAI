using MedAI.Contracts.Doctors;

namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorsController(IDoctorService doctorService,IDoctorAvailableTimeService doctorAvailableTimeService, IBookingService bookingService) : ControllerBase
{
    private readonly IDoctorService _doctorService = doctorService;
    private readonly IDoctorAvailableTimeService _doctorAvailableTimeService = doctorAvailableTimeService;
    private readonly IBookingService _bookingService = bookingService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllDoctors([FromQuery] RequestFilters filters,CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetAllAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDoctorById(int id,CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddDoctor([FromForm] AddDoctorRequest request,CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.AddDoctorAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDoctor(int id,[FromForm] UpdateDoctorRequest request,CancellationToken cancellationToken)
    {
        var result = await _doctorService.UpdateDoctorAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDoctor(int id,CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{doctorId}/available-times")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDoctorAvailableTimes(int doctorId,CancellationToken cancellationToken = default)
    {
        var result = await _doctorAvailableTimeService.GetByDoctorAsync(doctorId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("my-appointments")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyAppointments(CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetDoctorAppointmentsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}