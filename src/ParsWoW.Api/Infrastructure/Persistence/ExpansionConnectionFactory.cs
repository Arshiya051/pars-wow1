using Microsoft.Extensions.Options;
using MySqlConnector;
using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Persistence;

/// <summary>
/// Hands out fresh <see cref="MySqlConnection"/> instances for the three
/// databases of an expansion (Auth / Characters / World).
/// </summary>
public sealed class ExpansionConnectionFactory : IExpansionConnectionFactory
{
    private readonly IOptions<ParsWowOptions> _options;

    public ExpansionConnectionFactory(IOptions<ParsWowOptions> options)
    {
        _options = options;
    }

    public async Task<MySqlConnection> OpenAsync(ExpansionDatabase db, ExpansionKind expansion, CancellationToken ct = default)
    {
        if (!_options.Value.Connections.TryGetValue(expansion, out var conn))
            throw new InvalidOperationException($"No connection strings configured for {expansion}.");

        var cs = db switch
        {
            ExpansionDatabase.Auth => conn.Auth,
            ExpansionDatabase.Characters => conn.Characters,
            ExpansionDatabase.World => conn.World,
            _ => throw new ArgumentOutOfRangeException(nameof(db))
        };

        var connection = new MySqlConnection(cs);
        await connection.OpenAsync(ct).ConfigureAwait(false);
        return connection;
    }
}

public enum ExpansionDatabase { Auth, Characters, World }

public interface IExpansionConnectionFactory
{
    Task<MySqlConnection> OpenAsync(ExpansionDatabase db, ExpansionKind expansion, CancellationToken ct = default);
}
