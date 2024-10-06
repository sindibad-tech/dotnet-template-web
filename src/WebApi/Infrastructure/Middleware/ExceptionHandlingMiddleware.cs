using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Response;

namespace Sindibad.SAD.FlightInspection.WebApi.Infrastructure.Middleware;
public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env) : IMiddleware
{
    private readonly IHostEnvironment _env = env;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "failed api call");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(CreateDefaultFailedResponse(ex));
            }
        }
    }

    private ApiResponse CreateDefaultFailedResponse(Exception? ex)
    {
        List<string> errors = [];
        if (_env.IsDevelopment())
        {
            do
            {
                errors.Add(ex!.Message);
                ex = ex.InnerException;
            }
            while (ex is not null);
        }
        else
        {
            errors[0] = "Unhandled Server Error";
        }

        return new ApiResponse()
        {
            Errors = [.. errors],
            Success = false,
        };
    }
}
