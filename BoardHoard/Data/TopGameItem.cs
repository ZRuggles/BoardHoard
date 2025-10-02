namespace BoardHoard.Data;

public sealed class TopGameItem
{
    public string kind { get; set; } = "board"; // "board" | "card"
    public int id { get; set; }
    public string? title { get; set; }
}
