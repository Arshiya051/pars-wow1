using Microsoft.AspNetCore.Mvc;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Presentation.Controllers;

/// <summary>Controller for API health checks and subsystem diagnostics.</summary>
[ApiController]
[Route("api/diagnostics")]
[Produces("application/json")]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly IDbcProviderFactory _factory;
    /// <summary>Initialises a new <see cref="DiagnosticsController"/>.</summary>
    /// <param name="factory">DBC provider factory for provider status.</param>
    public DiagnosticsController(IDbcProviderFactory factory) => _factory = factory;

    /// <summary>Simple health check endpoint. Returns status OK with the current UTC time and list of registered expansions.</summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health() => Ok(new
    {
        status = "ok",
        timestamp = DateTimeOffset.UtcNow,
        expansions = _factory.Providers.Keys.Select(k => k.ToString()).ToArray()
    });

    /// <summary>Returns the DBC loading status for every registered expansion provider.</summary>
    /// <remarks>
    /// Shows which expansions are registered, their URL slug, whether their DBC files
    /// have been loaded successfully, and the list of files each provider expects.
    /// Useful for diagnosing startup failures.
    /// </remarks>
    [HttpGet("dbc/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DbcStatus() => Ok(new
    {
        utc = DateTimeOffset.UtcNow,
        providers = _factory.Providers.Values.Select(p => new
        {
            expansion = p.Expansion.ToString(),
            expansionSlug = p.Expansion.ToUrlSlug(),
            loaded = p.IsLoaded,
            files = p.RequiredFiles
        })
    });
}
