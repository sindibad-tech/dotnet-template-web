using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Sindibad.SAD.WebTemplate.WebApi.Infrastructure.FeatureFlags;
using Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Response;
using Sindibad.SAD.WebTemplate.WebApi.ViewModels.Dev;

namespace Sindibad.SAD.WebTemplate.WebApi.Controllers.V1;

public class DevController(ILogger<ApiControllerBase> logger, IFeatureManager featureManager) : ApiControllerBase(logger, featureManager)
{
    /// <summary>
    /// Get date and time
    /// </summary>
    /// <param name="type">the type of time</param>
    /// <returns>the requested time</returns>
    /// <remarks>This api is for testing purposes</remarks>
    [ProducesResponseType<ApiResponse>(StatusCodes.Status200OK)]
    [FeatureGate(FeatureNames.DevOnly)]
    [HttpGet("time")]
    public Task<ActionResult<ApiResponse<DateTimeOffset>>> Date([FromQuery] DatetimeType type) =>
        Task.FromResult<ActionResult<ApiResponse<DateTimeOffset>>>(Ok(new ApiResponse<DateTimeOffset>()
        {
            Success = true,
            Data = type switch
            {
                DatetimeType.Local => DateTimeOffset.Now,
                DatetimeType.UTC => DateTimeOffset.UtcNow,
                _ => DateTimeOffset.MinValue,
            },
        }));
}
