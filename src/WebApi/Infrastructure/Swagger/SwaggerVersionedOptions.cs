using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Swagger;

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
                Title = "Sindibad Search & Discovery Flight Inspection Service",
                License = new OpenApiLicense() { Name = "MIT" },
                Description = version.IsDeprecated switch
                {
                    true => "Warning! This Api version has been deprecated!",
                    false => "Sindibad Flight Inspection Api",
                },
                Version = version.ApiVersion.ToString(),
            });
        }
    }
}
