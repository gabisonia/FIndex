using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FIndex.Console.Dtos;

// Script to create and insert images into Qdrant db

var imageFolder = "faces";
var embedUrl = "http://localhost:8000/embed";
var qdrantHost = "http://localhost:6333";
var qdrantInsertUrl = $"{qdrantHost}/collections/faces/points";
var qdrantCollectionUrl = $"{qdrantHost}/collections/faces";

var http = new HttpClient();

// Check if 'faces' collection exists
var collectionsResponse = await http.GetAsync($"{qdrantHost}/collections");
if (!collectionsResponse.IsSuccessStatusCode)
{
    Console.WriteLine($"Failed to query Qdrant collections: {collectionsResponse.StatusCode}");
    return;
}

var collectionsJson = await collectionsResponse.Content.ReadAsStringAsync();
var existing = JsonSerializer.Deserialize<CollectionListResponse>(collectionsJson);

if (existing?.Result.Collections.Any(c => c.Name == "faces") == true)
{
    Console.WriteLine($"Qdrant collection 'faces' already exists.");
}
else
{
    var createBody = new
    {
        vectors = new
        {
            size = 512,
            distance = "Cosine"
        }
    };

    var createJson = new StringContent(JsonSerializer.Serialize(createBody), Encoding.UTF8, "application/json");
    var createResponse = await http.PutAsync(qdrantCollectionUrl, createJson);

    if (createResponse.IsSuccessStatusCode)
        Console.WriteLine("Qdrant collection 'faces' created.");
    else
    {
        var error = await createResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Failed to create collection: {error}");
        return;
    }
}

if (!Directory.Exists(imageFolder))
{
    Console.WriteLine($"Folder not found: {imageFolder}");
    return;
}

foreach (var file in Directory.GetFiles(imageFolder))
{
    var extension = Path.GetExtension(file).ToLower();
    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        continue;

    var fileName = Path.GetFileName(file);
    var name = Path.GetFileNameWithoutExtension(file);
    var uuid = Guid.NewGuid().ToString(); // valid Qdrant Id

    Console.WriteLine($"Processing: {fileName}");

    try
    {
        var bytes = await File.ReadAllBytesAsync(file);
        using var form = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(imageContent, "file", fileName);

        // Embed the image
        var embedResponse = await http.PostAsync(embedUrl, form);
        if (!embedResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Embed failed: {embedResponse.StatusCode}");
            continue;
        }

        var embedJson = await embedResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(embedJson);

        if (result?.Embedding is not { Length: 512 })
        {
            Console.WriteLine("Invalid embedding returned.");
            continue;
        }

        // Save to Qdrant
        var insertBody = new
        {
            points = new[]
            {
                new
                {
                    id = uuid,
                    vector = result.Embedding,
                    payload = new
                    {
                        name,
                        image = fileName
                    }
                }
            }
        };

        var qdrantJson = new StringContent(JsonSerializer.Serialize(insertBody), Encoding.UTF8, "application/json");
        var qdrantResponse = await http.PutAsync(qdrantInsertUrl, qdrantJson);

        if (qdrantResponse.IsSuccessStatusCode)
            Console.WriteLine($"Saved to Qdrant: {fileName}");
        else
        {
            var error = await qdrantResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Insert failed for {fileName}: {qdrantResponse.StatusCode}\n{error}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing {fileName}: {ex.Message}");
    }
}