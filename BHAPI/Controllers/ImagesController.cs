// Controllers/ImagesController.cs
using BoardHoardApi.Data;            // GamesDbContext
using BoardHoardAPI.Data;            // ImageBlob
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly GamesDbContext _db;
    private static readonly string[] Allowed = [".png", ".jpg", ".jpeg", ".webp"];

    public ImagesController(GamesDbContext db) => _db = db;

    public record ImageUploadResult(int ImageId, string Url);
    public class ImageUploadRequest
    {
        public IFormFile? File { get; set; }
    }

    [HttpPost]
    [RequestSizeLimit(5_000_000)] // 5MB cap; adjust if needed
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImageUploadResult>> Upload([FromForm] ImageUploadRequest request, CancellationToken ct)
    {
        var file = request.File;
        if (file is null || file.Length == 0) return BadRequest("Empty file.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return BadRequest("Only png/jpg/jpeg/webp.");

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var blob = new ImageBlob
        {
            FileName = file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            Length = file.Length,
            Data = ms.ToArray()
        };

        _db.Images.Add(blob);
        await _db.SaveChangesAsync(ct);

        // Return an API URL you can use directly in <img src="...">
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new ImageUploadResult(blob.Id, $"{baseUrl}/api/images/{blob.Id}"));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var img = await _db.Images.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        if (img is null) return NotFound();

        Response.Headers.CacheControl = "public, max-age=86400"; // cache 1 day
        return File(img.Data, img.ContentType);
    }
}
