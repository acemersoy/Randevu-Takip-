using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RandevuTakip.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        
        // Sadece randevu oluşturma uç noktasını sınırla (PublicController -> CreateAppointment)
        // URL formatı: api/{slug}/appointments
        if (context.Request.Method == "POST" && path != null && path.Contains("/appointments") && !path.Contains("/admin/"))
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = $"RateLimit_{ipAddress}";

            var cachedValue = await _cache.GetStringAsync(cacheKey);
            int count = cachedValue != null ? int.Parse(cachedValue) : 0;

            if (count >= 5) // Saatlik 5 randevu sınırı
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                var errorResponse = new { Message = "Çok fazla randevu talebi gönderdiniz. Lütfen bir saat sonra tekrar deneyin." };
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                return;
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            
            await _cache.SetStringAsync(cacheKey, (count + 1).ToString(), options);
        }

        await _next(context);
    }
}
