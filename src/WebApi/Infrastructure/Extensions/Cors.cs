using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sindibad.SAD.WebTemplate.WebApi.ConfigModels;
using Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Constants;

namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Extensions;
public static class Cors
{

    private const string CORS_SECTION = "Cors";

    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddCors(cors =>
        {
            var corsConfig = configuration.GetSection(CORS_SECTION).Get<CorsConfig>();

            cors.AddPolicy(CorsPolicyNames.ALLOW_ALL, policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            cors.AddPolicy(CorsPolicyNames.ALLOW_CONFIGURED, policy =>
                    policy
                        .WithMethods(corsConfig?.AllowedMethods ?? ["*"])
                        .WithOrigins(corsConfig?.AllowedOrigins ?? ["*"])
                        .WithHeaders(corsConfig?.AllowedHeaders ?? ["*"])
                );
        });

    }

    public static void ConfigureAppCors(this WebApplication app)
    {
        var corsConfig = app.Configuration.GetSection(CORS_SECTION).Get<CorsConfig>();

        if (corsConfig?.Enabled == true)
        {
            app.UseCors(CorsPolicyNames.ALLOW_CONFIGURED);
        }
        else
        {
            app.UseCors(CorsPolicyNames.ALLOW_ALL);
        }
    }
}
