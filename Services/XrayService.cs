using MedAI.Contracts.Xrays;

namespace MedAI.Services;

public class XrayService(
    ApplicationDbContext context,
    IHttpClientFactory httpClientFactory,
    IWebHostEnvironment env,
    IHttpContextAccessor httpContextAccessor) : IXrayService
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
            .AnyAsync(x => x.PatientId == userId && !x.IsEdited && !x.IsApproved, cancellationToken);

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

        using var ms = new MemoryStream();
        await request.Image.CopyToAsync(ms, cancellationToken);
        var base64Image = Convert.ToBase64String(ms.ToArray());

        var client = _httpClientFactory.CreateClient("AI");

        var payload = new { image = base64Image };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var modelResponse = await client.PostAsync("predict", content, cancellationToken);

        if (!modelResponse.IsSuccessStatusCode)
            return Result.Failure<UploadResponse>(XrayErrors.AIServiceUnavailable);

        var responseBody = await modelResponse.Content.ReadAsStringAsync(cancellationToken);

        string? aiDiagnosis = null;
        decimal? aiConfidence = null;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            aiDiagnosis = doc.RootElement.TryGetProperty("class", out var c) ? c.GetString() : null;
            aiConfidence = doc.RootElement.TryGetProperty("confidence", out var conf) ? (decimal?)conf.GetDouble() : null;
        }
        catch
        {
            return Result.Failure<UploadResponse>(XrayErrors.InvalidAIResponse);
        }

        var xray = new Xray
        {
            ImageUrl = $"/uploads/{fileName}",
            PatientId = userId,
            AI_Diagnosis = aiDiagnosis,
            AI_Confidence = aiConfidence,
            FinalDiagnosis = null,
            DoctorId = null,        // ✅ شيلنا FinalConfidence = null
            IsRevised = false
        };

        _context.Xrays.Add(xray);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new UploadResponse(
            xray.Id,
            xray.ImageUrl,
            xray.AI_Diagnosis,
            xray.AI_Confidence,     // ✅ بس AI_Confidence
            xray.FinalDiagnosis,
            xray.IsRevised,
            xray.CreatedAt,
            xray.ConfirmedAt,
            xray.PatientId,
            xray.DoctorId
        );

        return Result.Success(response);
    }

    public async Task<Result<PaginatedList<UnrevisedXrayResponse>>> GetUnrevisedXraysAsync(RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var query = _context.Xrays
            .AsNoTracking()
            .Where(x => !x.IsEdited && !x.IsApproved)
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
        // ✅ شيلنا FinalConfidence — الكونفدنس بيفضل من الـ AI
        xray.DoctorNotes = request.DoctorNotes;
        xray.DoctorId = doctor.Id;
        xray.IsRevised = true;
        xray.ConfirmedAt = DateTime.UtcNow;

        xray.IsApproved = string.Equals(request.FinalDiagnosis, xray.AI_Diagnosis, StringComparison.OrdinalIgnoreCase);
        xray.IsEdited = !xray.IsApproved;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<XrayResultResponse>> GetConfirmedXrayByIdAsync(int xrayId, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<XrayResultResponse>(UserErrors.InvalidJwtToken);

        var xray = await _context.Xrays
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == xrayId && x.PatientId == userId, cancellationToken);

        if (xray is null)
            return Result.Failure<XrayResultResponse>(XrayErrors.NotFound);

        if (!xray.IsRevised)
            return Result.Failure<XrayResultResponse>(XrayErrors.NotRevisedYet);

        var response = new XrayResultResponse(
            xray.Id,
            xray.ImageUrl,
            xray.FinalDiagnosis,
            xray.AI_Confidence,     // ✅ بدل FinalConfidence
            xray.DoctorNotes,
            xray.ConfirmedAt!.Value
        );

        return Result.Success(response);
    }

    public async Task<Result<PaginatedList<PatientXrayHistoryResponse>>> GetMyHistoryAsync(
        RequestFilters filters,
        CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<PaginatedList<PatientXrayHistoryResponse>>(UserErrors.InvalidJwtToken);

        var query = _context.Xrays
            .AsNoTracking()
            .Where(x => x.PatientId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PatientXrayHistoryResponse(
                x.Id,
                x.ImageUrl,
                x.IsRevised ? x.AI_Diagnosis : null,
                x.IsRevised ? x.AI_Confidence : null,   
                x.FinalDiagnosis,
                x.DoctorNotes,                          
                x.IsRevised,
                x.CreatedAt,
                x.ConfirmedAt
            ));

        var paginatedList = await PaginatedList<PatientXrayHistoryResponse>
            .CreateAsync(query, filters.PageNumber, filters.PageSize);

        return Result.Success(paginatedList);
    }
}