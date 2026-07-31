using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Application.Configuration;

/// <summary>
/// Root configuration for the Pars-WoW master API. Bound from configuration
/// section <c>ParsWow</c>.
/// </summary>
public sealed class ParsWowOptions
{
    public const string SectionName = "ParsWow";

    /// <summary>Logical environment (Development, Staging, Production).</summary>
    public string Environment { get; set; } = "Production";

    public DbcOptions Dbc { get; set; } = new();
    public JwtOptions Jwt { get; set; } = new();
    public AuthOptions Auth { get; set; } = new();
    public ShopOptions Shop { get; set; } = new();

    /// <summary>Per-expansion toggles and metadata.</summary>
    public Dictionary<ExpansionKind, ExpansionOptions> Expansions { get; set; } = new();

    /// <summary>Per-expansion MySQL connection strings (Auth/Characters/World).</summary>
    public Dictionary<ExpansionKind, ExpansionConnections> Connections { get; set; } = new();
}

public sealed class DbcOptions
{
    /// <summary>Filesystem root containing TBC/, WOTLK/, … subfolders.</summary>
    public string RootPath { get; set; } = "DBC";

    /// <summary>If true the API fails to start when a required DBC is missing.</summary>
    public bool FailFastOnMissing { get; set; } = true;

    /// <summary>Reload DBC files on filesystem change (dev/test only).</summary>
    public bool ReloadOnChange { get; set; }
}

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "ParsWoW";
    public string Audience { get; set; } = "ParsWoWClients";

    /// <summary>Symmetric signing secret. Loaded from secrets in production.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 30;
    public int RefreshTokenLifetimeDays { get; set; } = 14;
}

public sealed class AuthOptions
{
    /// <summary>Pepper appended to every password before hashing.</summary>
    public string Pepper { get; set; } = string.Empty;

    public bool AllowRegistration { get; set; } = true;

    public int MinUsernameLength { get; set; } = 3;
    public int MaxUsernameLength { get; set; } = 16;

    public int MinPasswordLength { get; set; } = 8;
    public int MaxPasswordLength { get; set; } = 16;

    /// <summary>If true, register accounts in BattleNet emulator mode (v/s + sha_pass_hash).</summary>
    public bool BattleNetEmulatorMode { get; set; } = true;
}

public sealed class ShopOptions
{
    public string Currency { get; set; } = "USD";
    public bool RequireExternalPayment { get; set; } = true;
    public int DefaultGoldPerPurchaseUnit { get; set; } = 1000;
}

public sealed class ExpansionOptions
{
    public bool Enabled { get; set; }
    public bool Default { get; set; }
    public string RealmlistName { get; set; } = string.Empty;
    public int MaxCharacterLevel { get; set; }
    public string Core { get; set; } = string.Empty;
}

public sealed class ExpansionConnections
{
    public string Auth { get; set; } = string.Empty;
    public string Characters { get; set; } = string.Empty;
    public string World { get; set; } = string.Empty;
}
