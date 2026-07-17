using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<AuditClient> _logger;

    public AuditClient(HttpClient http, IConfiguration config, ILogger<AuditClient> logger)
    {
        _http   = http;
        _apiKey = config["AuditService:ApiKey"] ?? string.Empty;
        _logger = logger;
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
                using var res = await _http.SendAsync(req);
                res.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Audit hatası ana akışı etkilememeli; yalnızca uyarı olarak loglanır.
                _logger.LogWarning(ex, "Audit log gönderilemedi: {ServiceName}/{Action} {EntityName}/{EntityId}",
                    serviceName, action, entityName, entityId);
            }
        });
    }
}
