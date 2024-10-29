using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Http.Headers;

namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Middleware
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string Realm = "SindibadAPI";

        public BasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Value.Contains("api"))
            {
                await _next(context);
                return; // Important: return here to skip the rest of the middleware
            }

            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Realm}\"";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access Forbidden.");
                return;
            }

            var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
            var username = credentials[0];
            var password = credentials[1];

            // Validate the username and password (e.g., check against a database or a hardcoded list)
            if (!IsAuthorizedUser(username, password))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid credentials.");
                return;
            }

            await _next(context);
        }

        private bool IsAuthorizedUser(string username, string password)
        {
            // Replace this with your user validation logic (e.g., database lookup, etc.)
            return username == "Sindiboss" && password == "FindingSevenThievesInBGW!";
        }
    }
}