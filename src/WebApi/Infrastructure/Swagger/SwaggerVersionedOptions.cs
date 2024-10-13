using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Swagger;

public class SwaggerVersionedOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider, IHostEnvironment env) : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _versionProvider = apiVersionDescriptionProvider;
    private readonly IHostEnvironment _env = env;

    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var version in _versionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(version.GroupName, new OpenApiInfo()
            {
                Title = SwaggerContent.Title + $" ({_env.EnvironmentName}) ",
                License = new OpenApiLicense() { Name = SwaggerContent.LicenseName },
                Description = version.IsDeprecated switch
                {
                    true => SwaggerContent.DeprecationWarning,
                    false => SwaggerContent.Description,
                },
                Version = version.ApiVersion.ToString(),
            });
        }
    }
}
