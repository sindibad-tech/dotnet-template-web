# A simple web api template
### Note: This project is a Work in progress

## Spec

The project is an ASP.NET Core project using .NET 8

The template addes support for common enterprise scale concerns such as 
- Telemtry (Logging, Metrics, Tracing)
- Health Checks
- Configuration
- Feature Flags
- Api Versioning
- Api Documentation using _swagger_

> Note:
  Security Concerns such as _CORS_ or _AUTH_ have not been added to the base template

### Telemetry
The template uses __OpenTelemetry__ Used mainly with the _OTLP_ protocol exporter configure in the 'appsettings.{environment}.json file and looks like this

```json
"OpenTelemetry": {
    "Logging": {
      "Enabled": true,
      "Endpoint": "http://127.0.0.1:38889",
      "Headers": {
      }
    },
    "Tracing": {
      "Enabled": true,
      "Endpoint": "http://127.0.0.1:38889",
      "Headers": {
      }
    },
    "Metrics": {
      "Enabled": true,
      "Endpoint": "http://127.0.0.1:38889",
      "Headers": {
      }
    }
  }
```

Add your OTLP endpoint address and you can add your custom header values as a json dictionary in the _headers_ field

> NOTE <br/>
  You can use any other custom OpenTelemetry Exporter if you wish by changing this part of the program file and adding the extra exporter

```c#
private static void ConfigureOpenTelemetry(IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
...

services.AddOpenTelemetry()
            .WithTracing(tracing =>

...

if (config.Enabled)
{
    // delete this if you wont use otlp exporter

    tracing.AddOtlpExporter(otlp =>
    {
        otlp.Headers = config.JoinedHeaders;
        otlp.Endpoint = new Uri(config.Endpoint!);
        otlp.Protocol = OtlpExportProtocol.Grpc;
    });

    // Add your other exporter configurations here
}

...
```

### Custom Telemetry

- ##### Logging

The project uses __Serilog__ As the log _formatter_ and the otlp exporter as the log _exporter_
meaning you can use seilog's data enrichment capabilities alongside the logs being exported

configure serilog here

```c#
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
```

- ##### Metrics

To create your own custom metrics simply inject a default `Meter` class from your DI into your classes and create `Counter`s, `Histogram`s and other from the meter. the custom metrics are expoterd by default

- ##### Traces

To create custom Traces inject a default `ActivitySource` class into your classes from your DI and create `Activity` objects from it and run them.
the custom traces are exported by default

<hr />

### Health Checks

the app exposes 2 health check endpoints on 2 base reoutes

- `/health` Use this path for the __healthiness__ check

This path is configured to report the application health and returns succesful when all critically important comonents are working
It does not fail when the app is in a `Degraded` state to allow the app to self-heal


- `/ready` Use this path for __readiness__ check
This path is configured to report the application health and returns succesful when all critically and _none_ critically important comonents are working
This endpoint however does fail when the app is in a `Degraded` state to allow the orchestrator to cut off traffic until the app is ready again

#### Adding Health Checks

Most popular tools used with dotnet have an open source healthcheck package. Install the package and add the required configuration to this part of the code

```c#
public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
{
    // try chaining all future required health checks here for: external services, databases, etc
    services
        .AddHealthChecks();
}
```

<hr />

### Configuration

The app uses several sources for configuration including _Env Variables_, _appsettings.json_ files and _input args_

The configuration follows the microsoft default patter. Read More about them __[Here](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)__

<hr />

### Feature Flags
The app makes use of feature flags, by default features are defined in the `features.json` file as a dictionary of &lt;name&gt;: &lt;bool&gt; values
extra configuration cn also be applied. Read about them __[Here](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core#feature-flag-declaration)__

#### Usage:
Define your Feature names in the `features.json` file, Add the name in the `FeatureNames` file as _constants_.

```c#
public class FeatureNames
{
    public const string DevOnly = nameof(DevOnly);
    // add others here
}
```

On your endpoints you can add the `FeatureGate` Attribute and specify the required features

```c#
[FeatureGate(FeatureNames.DevOnly)]
[HttpGet("time")]
public Task<ActionResult<ApiResponse<DateTimeOffset>>> Date([FromQuery] DatetimeType type)
...
```

You can also access the `IFeatureManager` Interface in the abstract `ApiControllerBase` in its implementing controllers to check wheter a feature is active or not

> NOTE <br />
  You can disable feature management for your development without modifying the features.json file by setting the 
  `DEV_APP_FeatureFlags__Disabled` or `APP_FeatureFlags__Disabled` variables to `true`

  <hr />




