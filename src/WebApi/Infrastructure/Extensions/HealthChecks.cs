using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;

namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Extensions;
public static class HealthChecks
{
    #region Constants

    [StringSyntax("Route")]
    private const string HEALTH_CHECK_ROUTE = "/live";

    [StringSyntax("Route")]
    private const string READINESS_CHECK_ROUTE = "/ready";

    #endregion

    #region Configuration

    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        // try adding all future required health checks here for: external services, databases, etc
        services
            .AddHealthChecks();
    }

    public static void MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthinessRoute();
        app.MapReadiinessRoute();
    }

    private static void MapHealthinessRoute(this WebApplication app)
    {
        app.MapHealthChecks(HEALTH_CHECK_ROUTE, new HealthCheckOptions()
        {
            AllowCachingResponses = false, // healthchecks should never be cached
            ResponseWriter = GetResponseWriterForEnv(app.Environment),
            ResultStatusCodes = HealthStatuses,
        });
    }

    private static void MapReadiinessRoute(this WebApplication app)
    {
        app.MapHealthChecks(READINESS_CHECK_ROUTE, new HealthCheckOptions()
        {
            AllowCachingResponses = false, // healthchecks should never be cached
            ResponseWriter = GetResponseWriterForEnv(app.Environment),
            ResultStatusCodes = ReadinessStatuses,
        });
    }

    #endregion

    #region Util

    // healh allows successful response when degraded since app must self-heal
    private static readonly IDictionary<HealthStatus, int> HealthStatuses = new Dictionary<HealthStatus, int>()
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    };

    // rediness returns failed response on degraded to stop incoming traffic
    private static readonly IDictionary<HealthStatus, int> ReadinessStatuses = new Dictionary<HealthStatus, int>()
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status424FailedDependency,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    };


    private static Func<HttpContext, HealthReport, Task> GetResponseWriterForEnv(IHostEnvironment env) =>
        env.IsDevelopment() ? WriteDetailedHealthCheckResponse : WriteHealthCheckResponse;

    private static async Task WriteHealthCheckResponse(HttpContext http, HealthReport report)
    {
        try
        {
            http.Response.ContentType = MediaTypeNames.Application.Json;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = Enum.GetName(report.Status),
                Duration = report.TotalDuration,
                Entries = report.Entries.Select(s => new { Name = s.Key, Status = Enum.GetName(s.Value.Status) })
            });
        }
        catch
        {
            return;
        }
    }

    // write a better rsponse for failures when running in dev
    private static async Task WriteDetailedHealthCheckResponse(HttpContext http, HealthReport report)
    {
        try
        {
            var entries = report.Entries.Select(s => new
            {
                Name = s.Key,
                Status = Enum.GetName(s.Value.Status),
                Error = s.Value.Exception?.Message,
                s.Value.Duration,
            });

            http.Response.ContentType = MediaTypeNames.Application.Json;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = Enum.GetName(report.Status),
                Duration = report.TotalDuration,
                Entries = entries,
            });
        }
        catch
        {
            return;
        }
    }

    #endregion
}
