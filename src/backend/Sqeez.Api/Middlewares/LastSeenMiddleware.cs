using Microsoft.Extensions.Caching.Memory;
using Sqeez.Api.Data;
using System.Security.Claims;

namespace Sqeez.Api.Middlewares
{
    public class LastSeenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public LastSeenMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, SqeezDbContext dbContext)
        {
            // Only process authenticated users
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // Extract the User ID from the JWT claims.
                var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (long.TryParse(userIdString, out long userId))
                {
                    // Throttle updates using Memory Cache (Max 1 DB update per minute per user)
                    var cacheKey = $"LastSeen_{userId}";

                    if (!_cache.TryGetValue(cacheKey, out _))
                    {
                        var user = await dbContext.Students.FindAsync(userId);

                        if (user != null)
                        {
                            user.LastSeen = DateTime.UtcNow;
                            await dbContext.SaveChangesAsync();

                            _cache.Set(cacheKey, true, TimeSpan.FromMinutes(1));
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    public static class LastSeenMiddlewareExtensions
    {
        public static IApplicationBuilder UseLastSeenTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LastSeenMiddleware>();
        }
    }
}