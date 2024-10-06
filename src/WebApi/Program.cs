using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using OpenTelemetry.Resources;
using Microsoft.AspNetCore.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using Serilog.Exceptions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Builder;
using Sindibad.SAD.FlightInspection.WebApi.ConfigModels;
using Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Extensions;
using Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Middleware;
using Microsoft.FeatureManagement;
namespace Sindibad.SAD.FlightInspection.WebApi;
public class Program
{
    #region Main

    public static async Task Main(string[] args)
    {
        try
        {
            WebApplication app;
            {
                var builder = WebApplication.CreateBuilder(args);

                ConfigureConfiguration(builder.Configuration, builder.Environment);

                ConfigureLogging(builder.Logging, builder.Configuration, builder.Environment);

                ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

                builder
                    .Host
                    .ConfigureHostOptions(ConfigureHostOptions)
                    .UseConsoleLifetime(c => c.SuppressStatusMessages = false);

                builder
                    .WebHost
                    .ConfigureKestrel(ConfigureKestrelOptions);

                app = builder.Build();
            }

            ConfigureApp(app);

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("App crashed with: {0}", ex);
        }
    }

    #endregion /Main

    #region App Spec

    public static readonly Version? ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version;

    public static readonly string ApplicationName = typeof(Program).Assembly.GetName().Name!;

    public static readonly ResourceBuilder ApplicationResource = ResourceBuilder
        .CreateDefault()
        .AddService(serviceName: ApplicationName, serviceVersion: ApplicationVersion?.ToString());

    #endregion

    #region Configuration

    private static void ConfigureConfiguration(IConfigurationBuilder configuration, IHostEnvironment env)
    {
        configuration.AddJsonFile(config =>
        {
            config.Path = "features.json";
            config.Optional = true;
            config.ReloadOnChange = true;
        });

        configuration.AddEnvironmentVariables("APP_");
        if (env.IsDevelopment())
        {
            configuration.AddEnvironmentVariables("DEV_APP_");
        }
    }

    #endregion

    #region Kestrel

    private static void ConfigureKestrelOptions(WebHostBuilderContext ctx, KestrelServerOptions opt)
    {
        opt.AddServerHeader = false;

        if (ushort.TryParse(ctx.Configuration["Port"], out var port))
        {
            opt.ListenAnyIP(port);
        }
        else
        {
            opt.ListenAnyIP(80);
        }
    }

    #endregion

    #region HostOptions

    private static void ConfigureHostOptions(HostOptions options)
    {
        options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        options.ServicesStartConcurrently = false;
        options.ServicesStopConcurrently = true;
    }

    #endregion

    #region Logging

    private static void ConfigureLogging(ILoggingBuilder logging, IConfiguration configuration, IHostEnvironment env)
    {
        logging.ClearProviders();
        logging.AddOpenTelemetry(otel =>
        {
            var config = configuration
                .GetRequiredSection("OpenTelemetry")
                .GetRequiredSection("Logging")
                .Get<OpenTelemetryConfig>() ?? throw new ApplicationException("Please provide logging open telemetry configuration");

            otel
            .SetResourceBuilder(ApplicationResource);

            otel.IncludeScopes = true;
            otel.ParseStateValues = true;
            otel.IncludeFormattedMessage = true;

            if (config.Enabled)
            {
                otel.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri(config.Endpoint!);
                    otlp.Headers = config.JoinedHeaders;
                    otlp.Protocol = OtlpExportProtocol.Grpc;
                });
            }
        });
    }

    private static void ConfigureSerilog(IConfiguration configuration, IHostEnvironment env, IServiceProvider sp, LoggerConfiguration serilog)
    {
        serilog.WriteTo.Console(theme: AnsiConsoleTheme.Code);
        serilog
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithEnvironmentUserName();
    }

    #endregion

    #region Services

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddSerilog((sp, logging) => ConfigureSerilog(configuration, env, sp, logging), writeToProviders: true);

        services.ConfigureHealthChecks(configuration, env);

        services.AddControllers();

        services.AddFeatureManagement();

        services.ConfigureApiVersioning(configuration, env);

        if (env.IsDevelopment())
            services.ConfigureSwagger(configuration, env);

        services.AddSingleton<ExceptionHandlingMiddleware>();

        ConfigureOpenTelemetry(services, configuration, env);
        ConfigureHttpClient(services, configuration, env);
        ConfigureDbContext(services, configuration, env);
    }

    private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        /// Removed Supabase from EF Core
        //services.AddDbContext<SupabaseDbContext>((sp, options) =>
        //{
        //    var connString = configuration.GetConnectionString("Supabase");
        //    var timeout = configuration
        //        .GetSection("ConnectionStrings")
        //        .GetSection("QueryTimeout")
        //        .Get<TimeSpan>();
        //    options
        //    .UseNpgsql(connString, opt => opt.CommandTimeout(timeout switch
        //    {
        //        { TotalSeconds: > 0 and var time } => (int)time,
        //        _ => 60
        //    }))
        //    .EnableSensitiveDataLogging();
        //});
        ///
    }

    private static void ConfigureHttpClient(IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        // we can add http logging here but we should use tracing instead for recording http requests
        services
            .AddHttpClient<HttpClient>()
            .AddStandardResilienceHandler();
    }

    private static void ConfigureOpenTelemetry(IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddSingleton<ActivitySource>(sp =>
        {
            return new ActivitySource(ApplicationName, ApplicationVersion?.ToString());
        });

        services.AddSingleton<Meter>(sp => sp
            .GetRequiredService<IMeterFactory>()
            .Create(new MeterOptions(ApplicationName)
            {
                Version = ApplicationVersion?.ToString(),
            }));

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                var config = configuration
                    .GetRequiredSection("OpenTelemetry")
                    .GetRequiredSection("Tracing")
                    .Get<OpenTelemetryConfig>() ?? throw new ApplicationException("Please provide an open telemetry tracing config!");

                tracing
                .SetResourceBuilder(ApplicationResource)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation(c => c.RecordException = true)
                .AddEntityFrameworkCoreInstrumentation(opt =>
                {
                    opt.SetDbStatementForText = true;
                    opt.SetDbStatementForStoredProcedure = true;
                });

                if (config.Enabled)
                {
                    tracing.AddOtlpExporter(otlp =>
                    {
                        otlp.Headers = config.JoinedHeaders;
                        otlp.Endpoint = new Uri(config.Endpoint!);
                        otlp.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                var config = configuration
                    .GetRequiredSection("OpenTelemetry")
                    .GetRequiredSection("Metrics")
                    .Get<OpenTelemetryConfig>() ?? throw new ApplicationException("Please provide an open telemetry metrics config!");

                metrics
                .SetResourceBuilder(ApplicationResource)
                .AddMeter(ApplicationName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();

                if (config.Enabled)
                {
                    metrics.AddOtlpExporter(otlp =>
                    {
                        otlp.Headers = config.JoinedHeaders;
                        otlp.Endpoint = new Uri(config.Endpoint!);
                        otlp.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
            });
    }

    #endregion

    #region ConfigureApi

    private static void ConfigureApp(WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.ConfiureAppSwagger();

        app.UseRouting();

        app.MapHealthCheckEndpoints();

        app.MapControllers();
    }

    #endregion
}
