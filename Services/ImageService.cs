using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace BoardHoard.Services;

public class ImageService : IImageService
{
    private readonly HttpClient _http;

    public ImageService(HttpClient http) => _http = http;

    public async Task<ImageUploadResult?> UploadAsync(IBrowserFile file)
    {
        if (file is null || file.Size == 0) return null;

        // Create multipart form data
        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream(maxAllowedSize: 5_000_000); // 5MB limit
        using var streamContent = new StreamContent(fileStream);

        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.Name);

        try
        {
            var response = await _http.PostAsync("/api/images", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ImageUploadResult>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}