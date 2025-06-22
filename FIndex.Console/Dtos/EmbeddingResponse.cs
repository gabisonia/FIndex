using System.Text.Json.Serialization;

namespace FIndex.Console.Dtos;

public record EmbeddingResponse
{
    [JsonPropertyName("embedding")] public float[] Embedding { get; set; }
}