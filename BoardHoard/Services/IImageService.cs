using Microsoft.AspNetCore.Components.Forms;

namespace BoardHoard.Services;

public interface IImageService
{
    Task<ImageUploadResult?> UploadAsync(IBrowserFile file);
}

public record ImageUploadResult(int ImageId, string Url);