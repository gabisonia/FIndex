using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FIndex.Console.Dtos;

// Script to save faces in qdrant

var imageFolder = "faces";
var embedUrl = "http://localhost:8000/embed";
var qdrantHost = "http://localhost:6333";
var qdrantInsertUrl = $"{qdrantHost}/collections/faces/points";
var qdrantCollectionUrl = $"{qdrantHost}/collections/faces";

var http = new HttpClient();

await EnsureQdrantCollectionExists(http, qdrantHost, qdrantCollectionUrl);

if (!Directory.Exists(imageFolder))
{
    Console.WriteLine($"Folder not found: {imageFolder}");
    return;
}

foreach (var file in Directory.GetFiles(imageFolder))
{
    if (!IsValidImageFile(file))
        continue;

    await ProcessImageFileAsync(file, http, embedUrl, qdrantInsertUrl);
}

Console.WriteLine("Done.");

#region Helper methods

static async Task EnsureQdrantCollectionExists(HttpClient http, string qdrantHost, string collectionUrl)
{
    var collectionsResponse = await http.GetAsync($"{qdrantHost}/collections");
    if (!collectionsResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to query Qdrant collections: {collectionsResponse.StatusCode}");
        Environment.Exit(1);
    }

    var collectionsJson = await collectionsResponse.Content.ReadAsStringAsync();
    var existing = JsonSerializer.Deserialize<CollectionListResponse>(collectionsJson);

    if (existing?.Result.Collections.Any(c => c.Name == "faces") == true)
    {
        Console.WriteLine("Qdrant collection 'faces' already exists.");
        return;
    }

    var createBody = new
    {
        vectors = new { size = 512, distance = "Cosine" }
    };

    var createJson = new StringContent(JsonSerializer.Serialize(createBody), Encoding.UTF8, "application/json");
    var createResponse = await http.PutAsync(collectionUrl, createJson);

    if (createResponse.IsSuccessStatusCode)
        Console.WriteLine("Qdrant collection 'faces' created.");
    else
    {
        var error = await createResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Failed to create collection: {error}");
        Environment.Exit(1);
    }
}

static bool IsValidImageFile(string path)
{
    var ext = Path.GetExtension(path).ToLower();
    return ext is ".jpg" or ".jpeg" or ".png";
}

static async Task ProcessImageFileAsync(string file, HttpClient http, string embedUrl, string qdrantInsertUrl)
{
    var fileName = Path.GetFileName(file);
    var name = Path.GetFileNameWithoutExtension(file);
    var uuid = Guid.NewGuid().ToString();

    Console.WriteLine($"Processing: {fileName}");

    try
    {
        var embedding = await GetEmbeddingAsync(http, embedUrl, file, fileName);
        if (embedding == null)
        {
            Console.WriteLine("Invalid embedding returned.");
            return;
        }

        var insertSuccess = await InsertToQdrantAsync(http, qdrantInsertUrl, uuid, embedding, name, fileName);
        if (insertSuccess)
            Console.WriteLine($"Saved to Qdrant: {fileName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing {fileName}: {ex.Message}");
    }
}

static async Task<float[]?> GetEmbeddingAsync(HttpClient http, string embedUrl, string filePath, string fileName)
{
    var bytes = await File.ReadAllBytesAsync(filePath);

    using var form = new MultipartFormDataContent();
    var content = new ByteArrayContent(bytes);
    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
    form.Add(content, "file", fileName);

    var response = await http.PostAsync(embedUrl, form);
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Embed failed: {response.StatusCode}");
        return null;
    }

    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<EmbeddingResponse>(json);
    return result?.Embedding is { Length: 512 } ? result.Embedding : null;
}

static async Task<bool> InsertToQdrantAsync(HttpClient http, string insertUrl, string id, float[] vector, string name,
    string imageFile)
{
    var insertBody = new
    {
        points = new[]
        {
            new
            {
                id,
                vector,
                payload = new { name, image = imageFile }
            }
        }
    };

    var content = new StringContent(JsonSerializer.Serialize(insertBody), Encoding.UTF8, "application/json");
    var response = await http.PutAsync(insertUrl, content);

    if (response.IsSuccessStatusCode)
        return true;

    var error = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Insert failed for {imageFile}: {response.StatusCode}\n{error}");

    return false;
}

#endregion