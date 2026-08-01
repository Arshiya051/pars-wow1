using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Dbc.Providers;

/// <summary>MoP (EternalCore).</summary>
public sealed class MopDbcProvider : DbcProviderBase<MopDbcProvider>
{
    public MopDbcProvider(
        IEnumerable<IDbcSchema> schemas,
        IOptions<ParsWowOptions> options,
        ILogger<MopDbcProvider> logger) : base(schemas, options, logger) { }

    public override ExpansionKind Expansion => ExpansionKind.MOP;
}
