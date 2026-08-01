using ParsWoW.Api.Application.Abstractions.Auth;
using ParsWoW.Api.Application.Abstractions.Persistence;

namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// 100% BlizzCMS-compatible password hasher.
///
/// Uses the <b>HexVerifier</b> (SRP6 BigInteger.ModPow) for the <c>account</c>
/// table and the <b>BnetVerifier</b> (SHA256) for the <c>battlenet_accounts</c>
/// table — exactly matching the <c>Pars.Authentication</c> library behaviour.
///
/// Byte contract — account table:
/// <code>
///   s            = random 32 bytes                                    → 64 hex chars
///   identity     = SHA1(ASCII(UPPER(username) + ":" + UPPER(password)))
///   saltRev      = ReverseHex(s) → bytes
///   x            = BigInteger(ReverseHex(SHA1(saltRev || identity)))
///   v            = BigInteger.ModPow(7, x, N) → normalize → 32 bytes → 64 hex chars
///   sha_pass_has
/// </code>
///
/// Byte contract — battlenet_accounts table:
/// <code>
///   bnet_sha     = Reverse(SHA256(hex(SHA256(UPPER(email))) + ":" + UPPER(password)))
///                  → 64 hex chars
/// </code>
/// </summary>
public sealed class BlizzCmsEmulatorPasswordHasher : IPasswordHasher
{
    private readonly HexVerifier _accountHasher = new();
    private readonly BnetVerifier _bnetHasher = new();

    public PasswordMaterial Hash(string username, string password)
    {
        // --- account table: HexVerifier (SRP6) ---
        var accountRecord = new AccountRecord
        {
            Username = username,
            Email = string.Empty // email filled separately if battlenet is needed
        };
        _accountHasher.Generate(accountRecord, password);

        return new PasswordMaterial(
            VerifierHex: accountRecord.VerifierHex!,
            SaltHex: accountRecord.SaltHex!,
            LegacyShaPassHashHex: accountRecord.ShaPassHash,
            BattlenetShaPassHashHex: string.Empty, // filled in AuthService when email is known
            BattleNetEmulatorMode: true);
    }

    public bool Verify(string username, string password, PasswordMaterial stored)
    {
        var accountRecord = new AccountRecord
        {
            Username = username,
            ShaPassHash = stored.LegacyShaPassHashHex,
            VerifierHex = stored.VerifierHex,
            SaltHex = stored.SaltHex
        };

        // --- Try SRP6 v/s verification first ---
        if (_accountHasher.Verify(username, password, accountRecord))
            return true;

        // --- Fallback: compare sha_pass_hash directly ---
        var fallbackHash = HexVerifier.CalculateShaPassHash(username, password);
        return string.Equals(fallbackHash, stored.LegacyShaPassHashHex, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Storage material produced by <see cref="IPasswordHasher.Hash"/>.
/// Contains everything needed to recreate and verify a password hash
/// for both the game server (<c>account</c> table) and the API.
/// </summary>
public sealed record PasswordMaterial(
    string VerifierHex,
    string SaltHex,
    string LegacyShaPassHashHex,
    string BattlenetShaPassHashHex,
    bool BattleNetEmulatorMode);
