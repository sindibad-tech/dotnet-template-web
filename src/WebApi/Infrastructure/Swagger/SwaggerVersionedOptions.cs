using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Swagger;

public class SwaggerVersionedOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider) : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _versionProvider = apiVersionDescriptionProvider;
    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var version in _versionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(version.GroupName, new OpenApiInfo()
            {
                Title = SwaggerContent.Title,
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
