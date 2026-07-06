using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace HedefTakip.Shared.Services;

public interface IAuditClient
{
    void Send(string serviceName, string action, string? entityName = null,
              string? entityId = null, string? userId = null, string? details = null);
}

public class AuditClient : IAuditClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public AuditClient(HttpClient http, IConfiguration config)
    {
        _http   = http;
        _apiKey = config["AuditService:ApiKey"] ?? string.Empty;
    }

    public void Send(string serviceName, string action, string? entityName = null,
                     string? entityId = null, string? userId = null, string? details = null)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var payload = JsonSerializer.Serialize(new { serviceName, action, entityName, entityId, userId, details });
                using var req = new HttpRequestMessage(HttpMethod.Post, "/api/audit-logs")
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
                req.Headers.Add("X-Api-Key", _apiKey);
                await _http.SendAsync(req);
            }
            catch { /* audit hatası ana akışı etkilememeli */ }
        });
    }
}
