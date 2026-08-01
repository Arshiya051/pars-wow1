namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// Emulator password-hashing modes supported by the Pars.Authentication library.
/// Default is <c>Hex</c> (also known as OldTrinity / BlizzCMS Emulator mode),
/// which is the most commonly used WoW emulator format.
/// </summary>
public enum EmulatorType
{
    /// <summary>SRP6 with hex-encoded v/s (BlizzCMS Emulator, Trinity-based cores).</summary>
    Hex = 0,

    /// <summary>Pure SRP6 with BigInteger modular exponentiation.</summary>
    Srp6 = 1,

    /// <summary>Legacy TrinityCore SHA1-based hashing.</summary>
    OldTrinity = 2,

    /// <summary>Battle.net SHA256-based hashing (battlenet_accounts table).</summary>
    Bnet = 3
}
