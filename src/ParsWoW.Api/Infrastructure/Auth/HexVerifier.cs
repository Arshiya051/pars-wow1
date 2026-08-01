using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ParsWoW.Api.Application.Abstractions.Persistence;

namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// BlizzCMS Hex (Old Trinity) password verifier.
/// This is the most common WoW emulator password mode.
///
/// Formula:
/// <code>
///   s        = random 32 bytes → 64 hex chars
///   identity = SHA1(ASCII(UPPER(username) + ":" + UPPER(password)))
///   salt     = ReverseHex(s_hex) → bytes
///   x        = BigInteger(ReverseHex(hex(SHA1(salt || identity))))
///   v        = BigInteger.ModPow(7, x, N) → 32 bytes → 64 hex chars
///   sha_pass = SHA1(ASCII(UPPER(username) + ":" + UPPER(password))) → 40 hex chars
/// </code>
/// </summary>
public sealed class HexVerifier : IPasswordVerifier
{
    // SRP6 group parameters (32 bytes, big-endian)
    private static readonly BigInteger G = new(7);
    private static readonly BigInteger N =
        BigIntegerExtensions.HexToBigInteger(
            "894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7");

    public bool Verify(string username, string password, AccountRecord account)
    {
        if (string.IsNullOrWhiteSpace(account.SaltHex) || string.IsNullOrWhiteSpace(account.VerifierHex))
            return false;

        var computed = CalculateVerifier(username, password, account.SaltHex);
        return string.Equals(computed, account.VerifierHex, StringComparison.OrdinalIgnoreCase);
    }

    public void Generate(AccountRecord account, string password)
    {
        account.SaltHex = GenerateSalt();       // 32 bytes → 64 hex chars
        account.VerifierHex = CalculateVerifier(account.Username, password, account.SaltHex);

        // Legacy sha_pass_hash = SHA1(UPPER(username):UPPER(password)) → 40 hex chars
        account.ShaPassHash = CalculateShaPassHash(account.Username, password);
    }

    // ----------------------------------------------------------------
    // Public helpers (also used by OldTrinity + Srp6 fallback paths)
    // ----------------------------------------------------------------

    /// <summary>Generate a cryptographically random 32-byte salt as uppercase hex.</summary>
    public static string GenerateSalt()
    {
        byte[] salt = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(salt);
    }

    /// <summary>
    /// Compute the SRP6 verifier v.
    ///   identity = SHA1("USER:PASS")
    ///   x = BigInteger(ReverseHex(SHA1(ReverseHex(s) || identity)))
    ///   v = g^x % N  (g=7)
    /// Returns uppercase hex, normalized to 32 bytes (64 chars).
    /// </summary>
    public static string CalculateVerifier(string username, string password, string saltHex)
    {
        // 1. identity = SHA1(UPPER(username):UPPER(password))
        byte[] identity = SHA1.HashData(
            Encoding.ASCII.GetBytes($"{username.ToUpperInvariant()}:{password.ToUpperInvariant()}"));

        // 2. Reverse the salt hex (big → little endian) then concatenate with identity
        byte[] salt = Convert.FromHexString(BigIntegerExtensions.ReverseHex(saltHex));
        byte[] combined = BigIntegerExtensions.Concat(salt, identity);

        // 3. x = SHA1(combined), then reverse hex → BigInteger
        byte[] xHash = SHA1.HashData(combined);
        string xHex = BigIntegerExtensions.ReverseHex(Convert.ToHexString(xHash));
        BigInteger x = BigIntegerExtensions.HexToBigInteger(xHex);

        // 4. v = g^x mod N
        BigInteger verifier = BigInteger.ModPow(G, x, N);

        // 5. Return as 32-byte uppercase hex
        return BigIntegerExtensions.BigIntegerToHex(verifier);
    }

    /// <summary>
    /// Legacy SHA1 password hash.
    ///   sha_pass_hash = SHA1(UPPER(username):UPPER(password))
    /// Returns 40-char uppercase hex.
    /// </summary>
    public static string CalculateShaPassHash(string username, string password)
    {
        byte[] hash = SHA1.HashData(
            Encoding.ASCII.GetBytes($"{username.ToUpperInvariant()}:{password.ToUpperInvariant()}"));
        return Convert.ToHexString(hash);
    }
}
