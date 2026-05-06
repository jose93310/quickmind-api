using Microsoft.AspNetCore.Mvc;

namespace QuickMind.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private static readonly string[] AllowedVideoTypes = new[] { "video/mp4", "video/webm" };
    private const long MaxImageSize = 5 * 1024 * 1024; // 5MB
    private const long MaxVideoSize = 20 * 1024 * 1024; // 20MB

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("media")]
    public async Task<ActionResult<object>> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No se proporcionó ningún archivo" });

        bool isImage = AllowedImageTypes.Contains(file.ContentType);
        bool isVideo = AllowedVideoTypes.Contains(file.ContentType);

        if (!isImage && !isVideo)
            return BadRequest(new { error = "Tipo de archivo no permitido. Solo imágenes y videos MP4/WebM." });

        if (isImage && file.Length > MaxImageSize)
            return BadRequest(new { error = "La imagen no puede superar los 5MB" });

        if (isVideo && file.Length > MaxVideoSize)
            return BadRequest(new { error = "El video no puede superar los 20MB" });

        try
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{fileName}";
            var mediaType = isImage ? "image" : "video";

            return Ok(new
            {
                url = fileUrl,
                mediaType = mediaType,
                originalName = file.FileName,
                size = file.Length
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Error al subir archivo: {ex.Message}" });
        }
    }
}
