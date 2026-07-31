namespace ParsWoW.Api.Application.Constants;

/// <summary>
/// Strongly typed identifier for a WoW expansion. Each kind maps to its own
/// directory under <see cref="Application.Configuration.DbcOptions.RootPath"/>,
/// its own MySQL connection set, and its own DBC provider/schema bundle.
/// </summary>
public enum ExpansionKind
{
    TBC = 1,
    WOTLK = 2,
    CATA = 3,
    MOP = 4,
    LEGION = 5
}

public static class ExpansionKindExtensions
{
    /// <summary>Folder name holding DBC files for <paramref name="kind"/>.</summary>
    public static string ToFolderName(this ExpansionKind kind) => kind switch
    {
        ExpansionKind.TBC => "TBC",
        ExpansionKind.WOTLK => "WOTLK",
        ExpansionKind.CATA => "CATA",
        ExpansionKind.MOP => "MOP",
        ExpansionKind.LEGION => "LEGION",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown expansion")
    };

    /// <summary>Lowercase tag used in URL segments: <c>tbc</c>, <c>wotlk</c>, etc.</summary>
    public static string ToUrlSlug(this ExpansionKind kind) => kind switch
    {
        ExpansionKind.TBC => "tbc",
        ExpansionKind.WOTLK => "wotlk",
        ExpansionKind.CATA => "cata",
        ExpansionKind.MOP => "mop",
        ExpansionKind.LEGION => "legion",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown expansion")
    };

    /// <summary>Inverse of <see cref="ToUrlSlug"/>.</summary>
    public static bool TryParseSlug(string slug, out ExpansionKind kind)
    {
        switch ((slug ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "tbc":    kind = ExpansionKind.TBC;    return true;
            case "wotlk":
            case "wotl":   kind = ExpansionKind.WOTLK;  return true;
            case "cata":   kind = ExpansionKind.CATA;   return true;
            case "mop":    kind = ExpansionKind.MOP;    return true;
            case "legion": kind = ExpansionKind.LEGION; return true;
            default:       kind = default;             return false;
        }
    }
}
