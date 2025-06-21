using System.Text.Json.Serialization;

namespace FIndex.Api.Services.Dtos;

public record QdrantSearchResponse
{
    [JsonPropertyName("result")]
    public List<ResultItem> Result { get; set; }

    public class ResultItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("score")]
        public float Score { get; set; }

        [JsonPropertyName("payload")]
        public Dictionary<string, object> Payload { get; set; }
    }
}