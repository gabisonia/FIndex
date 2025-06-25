using Findex.Embedder.Services;
using Microsoft.AspNetCore.Mvc;

namespace Findex.Embedder.Controllers;

[ApiController]
[Route("[controller]")]
public class EmbedController(FaceEmbeddingService embeddingService) : ControllerBase
{
    [HttpPost("/embed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EmbedImage([FromForm] EmbedImageRequest request)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var imageData = ms.ToArray();

        var embedding = embeddingService.GetEmbedding(imageData);

        if (embedding == null)
            return BadRequest("Invalid image or no face detected");

        var norm = Math.Sqrt(embedding.Sum(x => x * x));
        var normalized = embedding.Select(x => x / norm).ToArray();

        return new JsonResult(new { embedding = normalized });
    }
}

public class EmbedImageRequest
{
    [FromForm(Name = "file")] public IFormFile? File { get; set; }
}