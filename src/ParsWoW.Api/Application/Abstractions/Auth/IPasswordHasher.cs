using ParsWoW.Api.Infrastructure.Auth;

namespace ParsWoW.Api.Application.Abstractions.Auth;

/// <summary>
/// Password hashing abstraction. The BlizzCMS emulator implementation
/// lives in <c>Infrastructure.Auth.BlizzCmsEmulatorPasswordHasher</c>;
/// future implementations can swap in pure SRP6 or BCrypt without
/// touching the rest of the auth subsystem.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Compute the storage material for a fresh account.</summary>
    PasswordMaterial Hash(string username, string password);

    /// <summary>Verify a candidate password against stored material.</summary>
    bool Verify(string username, string password, PasswordMaterial stored);
}
