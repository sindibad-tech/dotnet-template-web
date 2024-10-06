using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Net.Mime;

namespace Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Extensions;
public static class HealthChecks
{
    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services
            .AddHealthChecks();
    }

    public static void MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions()
        {
            AllowCachingResponses = false,

            ResponseWriter = app.Environment.IsDevelopment() ? WriteDetailedHealthCheckResponse : WriteHealthCheckResponse,

            ResultStatusCodes = new Dictionary<HealthStatus, int>()
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status424FailedDependency,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            }
        });
    }

    private static async Task WriteHealthCheckResponse(HttpContext http, HealthReport report)
    {
        try
        {
            http.Response.ContentType = MediaTypeNames.Application.Json;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = System.Enum.GetName(report.Status),
                Duration = report.TotalDuration,
                Entries = report.Entries.Select(s => new { Name = s.Key, Status = System.Enum.GetName(s.Value.Status) })
            });
        }
        catch
        {
            return;
        }
    }

    private static async Task WriteDetailedHealthCheckResponse(HttpContext http, HealthReport report)
    {
        try
        {
            var entries = report.Entries.Select(s => new
            {
                Name = s.Key,
                Status = System.Enum.GetName(s.Value.Status),
                Error = s.Value.Exception?.Message,
                s.Value.Duration,
            });

            http.Response.ContentType = MediaTypeNames.Application.Json;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = System.Enum.GetName(report.Status),
                Duration = report.TotalDuration,
                Entries = entries,
            });
        }
        catch
        {
            return;
        }
    }
}
