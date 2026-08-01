using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ParsWoW.Api.Application.Abstractions.Persistence;

namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// Pure WoW SRP6 verifier with slightly different byte ordering than HexVerifier.
/// Uses <c>BigInteger.Parse</c> (not BigIntegerExtensions) and does NOT reverse
/// the x-hash before converting to BigInteger.
///
/// Some older WoW cores (pre-Wrath) used this variant.
/// </summary>
public sealed class Srp6Verifier : IPasswordVerifier
{
    private static readonly BigInteger G = new(7);
    private static readonly BigInteger N =
        BigInteger.Parse(
            "894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7",
            NumberStyles.AllowHexSpecifier);

    public bool Verify(string username, string password, AccountRecord account)
    {
        if (string.IsNullOrWhiteSpace(account.SaltHex) || string.IsNullOrWhiteSpace(account.VerifierHex))
            return false;

        var computed = CalculateVerifier(username, password, account.SaltHex);
        return string.Equals(computed, account.VerifierHex, StringComparison.OrdinalIgnoreCase);
    }

    public void Generate(AccountRecord account, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(32);
        account.SaltHex = Convert.ToHexString(salt);
        account.VerifierHex = CalculateVerifier(account.Username, password, account.SaltHex);

        // SRP6 mode does not generate sha_pass_hash by default
        account.ShaPassHash = "";
    }

    private static string CalculateVerifier(string username, string password, string saltHex)
    {
        byte[] salt = Convert.FromHexString(saltHex);

        // identity = USER:PASS  (no uppercasing in pure SRP6)
        byte[] h1 = SHA1.HashData(Encoding.ASCII.GetBytes($"{username}:{password}"));

        // x = SHA1(salt || h1)
        byte[] data = BigIntegerExtensions.Concat(salt, h1);
        byte[] h2 = SHA1.HashData(data);

        // Reverse the hash (little-endian for BigInteger)
        Array.Reverse(h2);

        // Prepend 0 to ensure positive BigInteger
        byte[] positive = new byte[h2.Length + 1];
        Buffer.BlockCopy(h2, 0, positive, 0, h2.Length);

        BigInteger x = new BigInteger(positive);
        BigInteger verifier = BigInteger.ModPow(G, x, N);

        byte[] bytes = verifier.ToByteArray(isUnsigned: true, isBigEndian: false);

        if (bytes.Length < 32)
            Array.Resize(ref bytes, 32);

        return Convert.ToHexString(bytes);
    }
}
