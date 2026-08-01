using ParsWoW.Api.Application.Abstractions.Persistence;

namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// Contract for emulator-specific password hashing algorithms.
/// Each verifier knows how to <c>Generate</c> password material
/// onto a mutable <see cref="AccountRecord"/> and how to
/// <c>Verify</c> a candidate password against one.
/// </summary>
public interface IPasswordVerifier
{
    /// <summary>Verify a password against the stored material.</summary>
    bool Verify(string username, string password, AccountRecord account);

    /// <summary>Generate password material (s, v, sha_pass_hash, etc.) on the record.</summary>
    void Generate(AccountRecord account, string password);
}
