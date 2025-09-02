// IImageService.cs
using Microsoft.AspNetCore.Components.Forms;

namespace BoardHoard.Services;
public interface IImageService
{
    Task<string?> UploadAsync(IBrowserFile file, string? title, CancellationToken ct = default);
}
