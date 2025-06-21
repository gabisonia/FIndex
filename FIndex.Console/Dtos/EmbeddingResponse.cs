using System.Text.Json.Serialization;

public record EmbeddingResponse
{
    [JsonPropertyName("embedding")] public float[] Embedding { get; set; }
}