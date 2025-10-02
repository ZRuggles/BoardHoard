
namespace BoardHoardAPI.Data;
public class ImageBlob
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}