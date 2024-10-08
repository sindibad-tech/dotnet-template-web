using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Swagger;
using System.IO;
using System.Reflection;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Extensions;
public static class Swagger
{

    #region Configuration

    public static void ConfigureSwagger(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddSwaggerGen(swagger =>
        {
            var path = GetDocumentationFilePath();

            swagger.IncludeXmlComments(path);
            swagger.AddEnumsWithValuesFixFilters(options =>
            {
                options.IncludeDescriptions = true;
                options.ApplyParameterFilter = true;
                options.ApplySchemaFilter = true;
            });
        });

        services.ConfigureOptions<SwaggerVersionedOptions>();
    }

    public static void ConfiureAppSwagger(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
    }

    #endregion

    #region Util

    private static string GetDocumentationFilePath()
    {
        var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

        return Path.Combine(AppContext.BaseDirectory, xmlFileName);
    }

    #endregion
}
