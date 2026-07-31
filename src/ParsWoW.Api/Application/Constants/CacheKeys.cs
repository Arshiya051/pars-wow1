using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Cache;

/// <summary>Centralized cache key builders shared across services and providers.</summary>
public static class CacheKeys
{
    public const string DbcMissingProbe = "dbc:missing-probe";
    public const string DbcLoadStatus = "dbc:load-status";

    public static string DbcItem(ExpansionKind e, int entry) =>
        $"dbc:{e.ToUrlSlug()}:item:{entry}";

    public static string DbcSpell(ExpansionKind e, int id) =>
        $"dbc:{e.ToUrlSlug()}:spell:{id}";

    public static string DbcMap(ExpansionKind e, int id) =>
        $"dbc:{e.ToUrlSlug()}:map:{id}";

    public static string DbcArea(ExpansionKind e, int id) =>
        $"dbc:{e.ToUrlSlug()}:area:{id}";

    public static string DbcAchievement(ExpansionKind e, int id) =>
        $"dbc:{e.ToUrlSlug()}:achievement:{id}";

    public static string DbcItemAll(ExpansionKind e) =>
        $"dbc:{e.ToUrlSlug()}:item:all";

    public static string DbcSpellAll(ExpansionKind e) =>
        $"dbc:{e.ToUrlSlug()}:spell:all";

    public static string ArmoryCharacter(ExpansionKind e, string realm, string name) =>
        $"armory:{e.ToUrlSlug()}:character:{realm.ToLowerInvariant()}:{name.ToLowerInvariant()}";

    public static string ArmoryEquipment(ExpansionKind e, Guid characterGuid) =>
        $"armory:{e.ToUrlSlug()}:equipment:{characterGuid:N}";

    public static string AuthAccount(int id) =>
        $"auth:account:{id}";

    public static string AuthRefresh(Guid jti) =>
        $"auth:refresh:{jti:N}";
}
