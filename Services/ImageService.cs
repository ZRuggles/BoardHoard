// ImageService.cs
using System.Net.Http.Headers;
using BoardHoard.Services;
using Microsoft.AspNetCore.Components.Forms;

public class ImageService(HttpClient http) : IImageService
{
    private readonly HttpClient _http = http;

    public async Task<string?> UploadAsync(IBrowserFile file, string? title, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(maxAllowedSize: 10_000_000, ct);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "File", file.Name);
        if (!string.IsNullOrWhiteSpace(title))
            content.Add(new StringContent(title), "Title");

        var res = await _http.PostAsync("api/uploads", content, ct);
        if (!res.IsSuccessStatusCode) return null;

        var dto = await res.Content.ReadFromJsonAsync<UploadDto>(cancellationToken: ct);
        return dto?.Url; // e.g. "/images/uploads/ticket-to-ride-20250221....jpg"
    }

    private record UploadDto(string FileName, string Url);
}
