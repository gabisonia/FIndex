using System.Text;
using System.Text.Json;
using FIndex.Api.Services.Dtos;

namespace FIndex.Api.Services;

public class QdrantService(HttpClient client)
{
    private const string Collection = "faces";

    public async Task<SearchResult[]> SearchAsync(float[] vector)
    {
        var body = new
        {
            vector = vector,
            top = 3,
            with_payload = true
        };

        var response = await client.PostAsync($"/collections/{Collection}/points/search",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            // ToDo: Log
            return [];
        }

        var raw = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<QdrantSearchResponse>(raw);
        if (data?.Result == null)
        {
            // ToDo: Log
            return [];
        }

        return data.Result.Select(r => new SearchResult
        {
            Image = r.Payload.TryGetValue("image", out var imageVal) ? imageVal?.ToString() : "unknown",
            Score = r.Score
        }).ToArray();
    }
}