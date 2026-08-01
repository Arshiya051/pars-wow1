namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// Factory that returns the correct <see cref="IPasswordVerifier"/>
/// for a given <see cref="EmulatorType"/>.
///
/// Most WoW cores use <c>Hex</c> (a.k.a. OldTrinity), which is the
/// BlizzCMS default.  <c>Bnet</c> is only used for the external
/// <c>battlenet_accounts</c> table.
/// </summary>
public static class PasswordVerifierFactory
{
    public static IPasswordVerifier GetVerifier(EmulatorType emulator)
    {
        return emulator switch
        {
            EmulatorType.Hex         => new HexVerifier(),
            EmulatorType.Srp6        => new Srp6Verifier(),
            EmulatorType.OldTrinity  => new OldTrinityVerifier(),
            EmulatorType.Bnet        => new BnetVerifier(),
            _ => throw new ArgumentOutOfRangeException(nameof(emulator),
                    $"Unknown emulator type: {emulator}")
        };
    }
}
