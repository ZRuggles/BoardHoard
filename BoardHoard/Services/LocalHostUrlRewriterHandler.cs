using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoardHoard.Services
{
    /// <summary>
    /// Rewrites any absolute http(s)://localhost URLs inside JSON responses
    /// to the current HttpClient's host (e.g., your Cloudflare API hostname).
    /// </summary>
    public class LocalhostUrlRewriterHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.Content?.Headers?.ContentType?.MediaType == "application/json")
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrEmpty(json) &&
                    json.IndexOf("localhost", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Use the actual host we called (should be your Cloudflare API host).
                    var targetBase = $"{request.RequestUri!.Scheme}://{request.RequestUri.Authority}";

                    string Rewrite(string s) =>
                        s.Replace("https://localhost:7021", targetBase, System.StringComparison.OrdinalIgnoreCase)
                         .Replace("http://localhost:7021", targetBase, System.StringComparison.OrdinalIgnoreCase)
                         .Replace("https://localhost", targetBase, System.StringComparison.OrdinalIgnoreCase)
                         .Replace("http://localhost", targetBase, System.StringComparison.OrdinalIgnoreCase);

                    var rewritten = Rewrite(json);
                    if (!ReferenceEquals(rewritten, json))
                    {
                        response.Content = new StringContent(rewritten, Encoding.UTF8, "application/json");
                    }
                }
            }

            return response;
        }
    }
}
