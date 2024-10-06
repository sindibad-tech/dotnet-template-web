using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Swagger;
using System.IO;
using System.Reflection;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Extensions;

public static class Swagger
{
    public static void ConfigureSwagger(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddSwaggerGen(swagger =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            swagger.IncludeXmlComments(xmlPath);
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

}
