using System.Text.Json;

namespace FIndex.Api.Services;

public class EmbeddingService(HttpClient client)
{
    public async Task<float[]?> GetEmbeddingAsync(byte[] imageBytes)
    {
        using var content = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(imageBytes);
        content.Add(byteContent, "file", "image.jpg");

        var response = await client.PostAsync("/embed", content);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, float[]>>(json);

        return result?["embedding"];
    }
}