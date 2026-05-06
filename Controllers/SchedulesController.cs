using MedAI.Contracts.Schedules;

namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Doctor")]
public class SchedulesController(IScheduleService scheduleService) : ControllerBase
{
    private readonly IScheduleService _scheduleService = scheduleService;

    [HttpPost]
    public async Task<IActionResult> Add(AddScheduleRequest request, CancellationToken cancellationToken)
    {
        var result = await _scheduleService.AddAsync(request, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet()]
    public async Task<IActionResult> GetMySlots(CancellationToken cancellationToken)
    {
        var result = await _scheduleService.GetMySchedulesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _scheduleService.DeleteScheduleAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{id}/capacity")]
    public async Task<IActionResult> UpdateCapacity(int id, [FromBody] UpdateCapacityRequest request, CancellationToken cancellationToken)
    {
        var result = await _scheduleService.UpdateCapacityAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}
