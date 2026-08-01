using System.Security.Cryptography;
using System.Text;
using ParsWoW.Api.Application.Abstractions.Persistence;

namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// Battle.net SHA256 password verifier for the <c>battlenet_accounts</c> table.
///
/// Formula:
/// <code>
///   emailHash    = SHA256(UTF8(email.ToUpper()))                              → 32 bytes
///   emailHashHex = hex(emailHash)                                              → 64 chars
///   finalHash    = SHA256(UTF8(emailHashHex + ":" + password.ToUpper()))
///   Array.Reverse(finalHash)
///   result       = hex(finalHash)                                              → 64 chars
/// </code>
///
/// This is used ONLY for the <c>battlenet_accounts.sha_pass_hash</c> column.
/// The <c>account</c> table uses <see cref="HexVerifier"/> instead.
/// </summary>
public sealed class BnetVerifier : IPasswordVerifier
{
    public bool Verify(string username, string password, AccountRecord account)
    {
        var computed = GenerateHash(account.Email, password);
        return string.Equals(computed, account.ShaPassHash, StringComparison.OrdinalIgnoreCase);
    }

    public void Generate(AccountRecord account, string password)
    {
        // Bnet mode: v/s are empty, only sha_pass_hash is used
        account.ShaPassHash = GenerateHash(account.Email, password);
        account.VerifierHex = "";
        account.SaltHex = "";
    }

    /// <summary>Generate the Battle.net SHA256 hash.</summary>
    public static string GenerateHash(string email, string password)
    {
        email = email.ToUpperInvariant();

        byte[] emailHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(email));

        string emailHashHex = Convert.ToHexString(emailHash);

        byte[] finalHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(emailHashHex + ":" + password.ToUpperInvariant()));

        Array.Reverse(finalHash);

        return Convert.ToHexString(finalHash);
    }
}
