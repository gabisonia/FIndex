using FIndex.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIndex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController(EmbeddingService embeddingService, QdrantService qdrantService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Search(IFormFile? image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("Image is required");

        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);
        var imageBytes = ms.ToArray();

        var embedding = await embeddingService.GetEmbeddingAsync(imageBytes);
        if (embedding == null)
            return StatusCode(500, "Embedding generation failed");

        var results = await qdrantService.SearchAsync(embedding);
        return Ok(results);
    }
}