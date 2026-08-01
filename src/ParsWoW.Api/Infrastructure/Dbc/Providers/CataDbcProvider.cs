using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Dbc.Providers;

/// <summary>CATA (WoWSourceV10).</summary>
public sealed class CataDbcProvider : DbcProviderBase<CataDbcProvider>
{
    public CataDbcProvider(
        IEnumerable<IDbcSchema> schemas,
        IOptions<ParsWowOptions> options,
        ILogger<CataDbcProvider> logger) : base(schemas, options, logger) { }

    public override ExpansionKind Expansion => ExpansionKind.CATA;
}
