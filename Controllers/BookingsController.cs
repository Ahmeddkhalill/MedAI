namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    private readonly IBookingService _bookingService = bookingService;

    [HttpPost("{doctorAvailableTimeId}")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Create(int doctorAvailableTimeId, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CreateAsync(doctorAvailableTimeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("my-bookings")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyBookings([FromQuery] RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var result = await _bookingService.GetMyBookingsAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _bookingService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("my-appointments")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] RequestFilters filters,CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetDoctorAppointmentsAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
