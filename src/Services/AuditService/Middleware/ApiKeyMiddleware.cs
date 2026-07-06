namespace AuditService.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next  = next;
        _apiKey = configuration["ApiKey"]!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Yalnızca POST /api/audit-logs için API key kontrolü yapılır
        if (context.Request.Method == HttpMethods.Post &&
            context.Request.Path.StartsWithSegments("/api/audit-logs"))
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) ||
                providedKey != _apiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { success = false, userMessage = "Geçersiz API anahtarı." });
                return;
            }
        }

        await _next(context);
    }
}
