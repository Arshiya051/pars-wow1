using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Dbc;

/// <summary>Resolves an IDbcProvider for an expansion by slug or enum.</summary>
public sealed class DbcProviderFactory : IDbcProviderFactory
{
    private readonly IReadOnlyDictionary<ExpansionKind, IDbcProvider> _providers;

    public DbcProviderFactory(IEnumerable<IDbcProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Expansion);
    }

    public IReadOnlyDictionary<ExpansionKind, IDbcProvider> Providers => _providers;

    public IDbcProvider GetProvider(ExpansionKind kind)
    {
        if (_providers.TryGetValue(kind, out var p)) return p;
        throw new KeyNotFoundException($"No DBC provider registered for expansion {kind}.");
    }

    public bool TryResolve(string slug, out IDbcProvider provider)
    {
        provider = null!;
        if (ExpansionKindExtensions.TryParseSlug(slug, out var k)
            && _providers.TryGetValue(k, out var p))
        {
            provider = p;
            return true;
        }
        return false;
    }
}
