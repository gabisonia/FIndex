namespace FIndex.Api.Services.Dtos;

public record SearchResult
{
    public string? Image { get; set; }
    public float Score { get; set; }
    // ToDo
    public string? Title { get; set; } = "N/A";
}