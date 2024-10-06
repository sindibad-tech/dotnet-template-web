using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Constants;

namespace Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Extensions;
internal static class Versioning
{
    public static void ConfigureApiVersioning(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        var versioningConfig = configuration
            .GetSection("Versioning");

        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = GetDefaultApiVersion(versioningConfig);
            options.ReportApiVersions = true;
            options.RouteConstraintName = "apiVersion";
            options.UnsupportedApiVersionStatusCode = StatusCodes.Status501NotImplemented;
            options.ApiVersionReader = ApiVersionReader
                .Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version")
                );
        })
        .AddApiExplorer(options =>
        {
            options.DefaultApiVersion = GetDefaultApiVersion(configuration);
            options.GroupNameFormat = "'v'VV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    private static ApiVersion GetDefaultApiVersion(IConfiguration configSection)
    {
        double version = double.TryParse(configSection["DefaultVersion"], out var defaultVer) switch
        {
            true => defaultVer,
            false => VersionConstants.V1,
        };

        return new ApiVersion(version);
    }

}
