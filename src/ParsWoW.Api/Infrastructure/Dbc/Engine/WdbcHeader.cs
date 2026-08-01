namespace ParsWoW.Api.Infrastructure.Dbc.Engine;

/// <summary>
/// Header from a Blizzard WDBC/WDB binary file. Format documented at
/// <see href="https://wowdev.wiki/DBC"/>. Identical across expansions.
/// </summary>
public readonly record struct WdbcHeader(
    uint Magic,
    int RecordCount,
    int FieldCount,
    int RecordSize,
    int StringBlockSize)
{
    /// <summary>"WDBC" little-endian magic.</summary>
    public const uint WdbcMagic = 0x43424457;

    /// <summary>"WDB2" little-endian magic (used in WOTLK / CATA where file format is WDB2/WDB3/WDB4/WDB5 but the same parsing applies to WDBC).</summary>
    public const uint Wdb2Magic = 0x32424457;

    public bool IsValid => Magic is WdbcMagic or Wdb2Magic
        or 0x33424457     // WDB3
        or 0x34424457     // WDB4
        or 0x35424457;    // WDB5 — same physical struct layout
}
