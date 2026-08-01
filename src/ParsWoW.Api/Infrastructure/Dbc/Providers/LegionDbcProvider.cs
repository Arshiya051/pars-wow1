using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Dbc.Providers;

/// <summary>Legion (LegionCoreV2).</summary>
public sealed class LegionDbcProvider : DbcProviderBase<LegionDbcProvider>
{
    public LegionDbcProvider(
        IEnumerable<IDbcSchema> schemas,
        IOptions<ParsWowOptions> options,
        ILogger<LegionDbcProvider> logger) : base(schemas, options, logger) { }

    public override ExpansionKind Expansion => ExpansionKind.LEGION;
}
