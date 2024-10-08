using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Constants;

namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Extensions;
internal static class Versioning
{
    #region Constants

    private const string VERSION_GROUP_FORMAT = "'v'VV"; // this will support versioning for major and minor forms only ->  1.0, 1.1 OK | 1.0.1, 1.0.2 NOT OK

    private const string CONFIGURATION_SECTION_KEY = "Versioning";

    private const string CONFIGURATION_DEFAULT_VERSION_KEY = "DefaultVersion";

    private const string QUERY_STRING_KEY = "api-version";

    private const string HEADER_KEY = "X-api-version";

    #endregion

    #region Configuration

    public static void ConfigureApiVersioning(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        var versioningConfig = configuration
            .GetSection(CONFIGURATION_SECTION_KEY);

        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = GetDefaultApiVersion(versioningConfig);
            options.ReportApiVersions = true;
            options.UnsupportedApiVersionStatusCode = StatusCodes.Status501NotImplemented;
            options.ApiVersionReader = ApiVersionReader
                .Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader(QUERY_STRING_KEY),
                    new HeaderApiVersionReader(HEADER_KEY)
                );
        })
        .AddApiExplorer(options =>
        {
            options.DefaultApiVersion = GetDefaultApiVersion(configuration);
            options.GroupNameFormat = VERSION_GROUP_FORMAT;
            options.SubstituteApiVersionInUrl = true;
        });
    }

    private static ApiVersion GetDefaultApiVersion(IConfiguration configSection)
    {
        double version = double.TryParse(configSection[CONFIGURATION_DEFAULT_VERSION_KEY], out var defaultVer) switch
        {
            true => defaultVer,
            false => VersionConstants.V1,
        };

        return new ApiVersion(version);
    }

    #endregion
}
