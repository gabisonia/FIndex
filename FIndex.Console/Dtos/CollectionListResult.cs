using System.Text.Json.Serialization;

namespace FIndex.Console.Dtos;

public record CollectionListResult
{
    [JsonPropertyName("collections")]
    public List<CollectionInfo> Collections { get; set; }
}