using Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using Microsoft.FeatureManagement;

namespace Sindibad.SAD.WebTemplate.WebApi.Controllers;
[ApiController]
[Route("api/v{ver:apiVersion}/[controller]")]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[ApiVersion(VersionConstants.V1)]
public abstract class ApiControllerBase(ILogger<ApiControllerBase> logger, IFeatureManager featureManager) : ControllerBase
{
    protected ILogger<ApiControllerBase> Logger { get; } = logger;

    protected IFeatureManager FeatureManager { get; } = featureManager;
}
