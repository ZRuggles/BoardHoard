using System.Text.Json;

namespace BoardHoard.Data;

public static class TopGamesHelper
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string? BuildJson(IEnumerable<TopGameItem> items)
    {
        if (items is null) return null;
        // unique by kind|id, max 3
        var distinct = items
            .Where(i => i != null && !string.IsNullOrWhiteSpace(i.kind) && i.id > 0)
            .GroupBy(i => $"{i.kind}|{i.id}")
            .Select(g => g.First())
            .Take(3)
            .ToList();

        return distinct.Count == 0 ? null : JsonSerializer.Serialize(distinct, _json);
    }

    public static List<TopGameItem> ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<TopGameItem>>(json, _json) ?? new();
        }
        catch { return new(); }
    }
}
