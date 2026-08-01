using ParsWoW.Api.Application.Abstractions.Persistence;

namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// Legacy TrinityCore verifier — functionally identical to <see cref="HexVerifier"/>.
/// BlizzCMS uses this mode for "Old Trinity" cores that support SHA1 + v/s.
/// </summary>
public sealed class OldTrinityVerifier : IPasswordVerifier
{
    private readonly HexVerifier _inner = new();

    public bool Verify(string username, string password, AccountRecord account)
        => _inner.Verify(username, password, account);

    public void Generate(AccountRecord account, string password)
        => _inner.Generate(account, password);
}
