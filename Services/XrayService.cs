using MedAI.Contracts.Common;
using MedAI.Contracts.Xrays;
using MedAI.Extensions;
using System.Text;
using System.Text.Json;

namespace MedAI.Services;

public class XrayService(ApplicationDbContext context,IHttpClientFactory httpClientFactory,
    IWebHostEnvironment env,IHttpContextAccessor httpContextAccessor) : IXrayService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IWebHostEnvironment _env = env;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<UploadResponse>> UploadXrayAsync(UploadRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<UploadResponse>(UserErrors.InvalidJwtToken);

        var hasUnrevisedXray = await _context.Xrays
            .AnyAsync(x => x.PatientId == userId && !x.IsRevised, cancellationToken);

        if (hasUnrevisedXray)
            return Result.Failure<UploadResponse>(XrayErrors.PendingReview);

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.Image.CopyToAsync(stream, cancellationToken);
        }

        var imageBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        var base64Image = Convert.ToBase64String(imageBytes);

        var client = _httpClientFactory.CreateClient();
        var payload = new { image = base64Image };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var ModelResponse = await client.PostAsync("http://127.0.0.1:5000/predict", content, cancellationToken);
        var responseBody = await ModelResponse.Content.ReadAsStringAsync(cancellationToken);

        string? aiDiagnosis = null;
        decimal? aiConfidence = null;

        if (ModelResponse.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(responseBody);
            aiDiagnosis = doc.RootElement.GetProperty("class").GetString();
            aiConfidence = (decimal?)doc.RootElement.GetProperty("confidence").GetSingle();
        }

        var xray = new Xray
        {
            ImageUrl = $"uploads/{fileName}",
            PatientId = userId!,
            AI_Diagnosis = aiDiagnosis,
            AI_Confidence = aiConfidence,
            FinalDiagnosis = null,
            FinalConfidence = null,
            DoctorId = null,
            IsRevised = false
        };

        _context.Xrays.Add(xray);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new UploadResponse(
            Id: xray.Id,
            ImageUrl: xray.ImageUrl,
            AI_Diagnosis: xray.AI_Diagnosis,
            AI_Confidence: xray.AI_Confidence,
            FinalDiagnosis: xray.FinalDiagnosis,
            FinalConfidence: xray.FinalConfidence,
            IsRevised: xray.IsRevised,
            CreatedAt: xray.CreatedAt,
            ConfirmedAt: xray.ConfirmedAt,
            PatientId: xray.PatientId,
            DoctorId: xray.DoctorId
        );

        return Result.Success(response);
    }

    public async Task<Result<PaginatedList<UnrevisedXrayResponse>>> GetUnrevisedXraysAsync(RequestFilters filters,CancellationToken cancellationToken = default)
    {
        var query = _context.Xrays
            .AsNoTracking()
            .Where(x => !x.IsRevised)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new UnrevisedXrayResponse(
                x.Id,
                x.ImageUrl,
                x.AI_Diagnosis,
                x.AI_Confidence,
                x.PatientId,
                x.CreatedAt
            ));

        var response = await PaginatedList<UnrevisedXrayResponse>.CreateAsync(query, filters.PageNumber, filters.PageSize);

        return Result.Success(response);
    }

    public async Task<Result> ConfirmXrayAsync(int xrayId, ConfirmXrayRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure(DoctorErrors.NotFound);

        var xray = await _context.Xrays
            .FirstOrDefaultAsync(x => x.Id == xrayId, cancellationToken);

        if (xray is null)
            return Result.Failure(XrayErrors.NotFound);

        if (xray.IsRevised)
            return Result.Failure(XrayErrors.AlreadyRevised);

        xray.FinalDiagnosis = request.FinalDiagnosis;
        xray.FinalConfidence = request.FinalConfidence;
        xray.DoctorId = doctor.Id; 
        xray.IsRevised = true;
        xray.ConfirmedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
