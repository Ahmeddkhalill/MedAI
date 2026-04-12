using MedAI.Contracts.Xrays;

namespace MedAI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class XraysController(IXrayService xrayService) : ControllerBase
{
    private readonly IXrayService _xrayService = xrayService;

    [HttpPost("upload")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> UploadXray([FromForm] UploadRequest request, CancellationToken cancellationToken)
    {
        var result = await _xrayService.UploadXrayAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("unrevised")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetUnrevisedXrays([FromQuery] RequestFilters filters, CancellationToken cancellationToken)
    {
        var result = await _xrayService.GetUnrevisedXraysAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost("{xrayId}/confirm")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> ConfirmXray(int xrayId, [FromBody] ConfirmXrayRequest request, CancellationToken cancellationToken)
    {
        var result = await _xrayService.ConfirmXrayAsync(xrayId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{xrayId}")]
    [Authorize(Roles = "Doctor,Patient")]
    public async Task<IActionResult> GetConfirmedXrayById(int xrayId, CancellationToken cancellationToken)
    {
        var result = await _xrayService.GetConfirmedXrayByIdAsync(xrayId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("my-history")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyHistory([FromQuery] RequestFilters filters, CancellationToken cancellationToken)
    {
        var result = await _xrayService.GetMyHistoryAsync(filters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
