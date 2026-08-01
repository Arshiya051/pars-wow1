using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Dbc.Providers;

/// <summary>WOTLK (mangoswotlk).</summary>
public sealed class WotlkDbcProvider : DbcProviderBase<WotlkDbcProvider>
{
    public WotlkDbcProvider(
        IEnumerable<IDbcSchema> schemas,
        IOptions<ParsWowOptions> options,
        ILogger<WotlkDbcProvider> logger) : base(schemas, options, logger) { }

    public override ExpansionKind Expansion => ExpansionKind.WOTLK;
}
