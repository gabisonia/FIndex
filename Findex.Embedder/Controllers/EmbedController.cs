using Findex.Embedder.Services;
using Microsoft.AspNetCore.Mvc;

namespace Findex.Embedder.Controllers;

[ApiController]
[Route("[controller]")]
public class EmbedController() : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> EmbedImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var embeddingService = new FaceEmbeddingService();
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var imageData = ms.ToArray();

        var embedding = embeddingService.GetEmbedding(imageData);

        if (embedding == null)
            return BadRequest("Invalid image or no face detected");

        return Ok(new { embedding });
    }
}