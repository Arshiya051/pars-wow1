using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Application.Abstractions.Dbc;

/// <summary>
/// Factory that resolves the correct IDbcProvider for a given expansion.
/// </summary>
public interface IDbcProviderFactory
{
    /// <summary>All enabled providers keyed by expansion.</summary>
    IReadOnlyDictionary<ExpansionKind, IDbcProvider> Providers { get; }

    /// <summary>Get the provider for <paramref name="kind"/> or throw if disabled.</summary>
    IDbcProvider GetProvider(ExpansionKind kind);

    /// <summary>Try to resolve an expansion from a URL slug (e.g. <c>"wotlk"</c>).</summary>
    bool TryResolve(string slug, out IDbcProvider provider);
}
