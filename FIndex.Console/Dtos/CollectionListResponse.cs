using System.Text.Json.Serialization;

namespace FIndex.Console.Dtos;

public record CollectionListResponse
{
    [JsonPropertyName("result")]
    public CollectionListResult Result { get; set; }
}