using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Dbc.Providers;

/// <summary>
/// TBC (OregonCore). Uses ONLY its own schemas under <c>Schemas/TBC/</c>.
/// </summary>
public sealed class TbcDbcProvider : DbcProviderBase<TbcDbcProvider>
{
    public TbcDbcProvider(
        IEnumerable<IDbcSchema> schemas,
        IOptions<ParsWowOptions> options,
        ILogger<TbcDbcProvider> logger) : base(schemas, options, logger) { }

    public override ExpansionKind Expansion => ExpansionKind.TBC;
}
