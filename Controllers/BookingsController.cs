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
}
