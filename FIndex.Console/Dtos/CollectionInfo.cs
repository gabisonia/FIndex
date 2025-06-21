using System.Text.Json.Serialization;

namespace FIndex.Console.Dtos;

public record CollectionInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}