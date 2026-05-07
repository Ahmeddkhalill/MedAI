namespace MedAI.Contracts.Common;

public record RequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchValue { get; set; }
    public string? Type { get; set; } 
}
public record ListResponse<T>(
    int TotalCount,
    List<T> Items
);