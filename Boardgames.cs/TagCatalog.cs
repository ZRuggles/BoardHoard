using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardgames.Shared
{
    public static class TagCatalog
    {
        // key -> (storedValue, uiLabel, tooltip)
        public static readonly (string Key, (string Value, string Label, string Hint)[] Options)[] Groups =
        {
        ("intelligence", new[] { ("smart","Smart","Thinky / cerebral"), ("dumb","Dumb","Turn-your-brain-off fun") }),
        ("closeness", new[] { ("friends","Friends","Comfortable group"), ("not-friends","Not Friends","Mixed / new people") }),
        ("competitiveness", new[] { ("tryhard","Tryhard","Play to win"), ("troll","Troll","Play for chaos / laughs") }),
        ("time", new[] { ("short-<30","< 30 min","Quick round"), ("long-≥30","≥ 30 min","Longer session") }),
        ("heaven", new[] { ("degenerate","Degenerate","Borderline / spicy content"), ("normie","Normie","Family-friendly / safe") }),
        ("rules", new[] { ("more-rules","More Rules","Heavier rules"), ("less-rules","Less Rules","Light rules") }),
        ("rng", new[] { ("less-rng","Less RNG","Skill-heavy"), ("more-rng","More RNG","Chaos / luck swings") }),
        ("tier", new[] { ("S","S","Top tier"), ("A","A","Great"), ("B","B","Good") }),
    };

        public static string Compose(string key, string value) => $"{key}:{value}";
        public static string? ParseKey(string tag) => tag.Contains(':') ? tag.Split(':', 2)[0] : null;
        public static string? ParseValue(string tag) => tag.Contains(':') ? tag.Split(':', 2)[1] : null;

        public static Dictionary<string, string> ToSelectionMap(IEnumerable<string> tags)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in tags ?? Array.Empty<string>())
            {
                var k = ParseKey(t); var v = ParseValue(t);
                if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(v)) map[k!] = v!;
            }
            return map;
        }

        public static List<string> FromSelectionMap(Dictionary<string, string> sel)
            => sel.Select(kv => Compose(kv.Key, kv.Value)).ToList();

        // Look up the UI label + hint for a stored tag like "rules:more-rules"
        public static (string Label, string Hint)? Describe(string tag)
        {
            var k = ParseKey(tag); var v = ParseValue(tag);
            if (k is null || v is null) return null;
            var group = Groups.FirstOrDefault(g => g.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
            if (group.Key is null) return null;
            var opt = group.Options.FirstOrDefault(o => o.Value.Equals(v, StringComparison.OrdinalIgnoreCase));
            return opt.Value is null ? null : (opt.Label, opt.Hint);
        }
    }
}
