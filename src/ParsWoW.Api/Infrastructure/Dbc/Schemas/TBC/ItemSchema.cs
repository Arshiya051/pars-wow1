using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.TBC;

/// <summary>
/// TBC (2.x.x) Item.dbc column layout. Column offsets adapted from the
/// 2.x entry on <see href="https://wowdev.wiki/DBC/Item"/>. Only the
/// columns the API cores surface today are projected; the full raw
/// record remains available through <see cref="DbcRecord"/> for ad-hoc
/// lookups by callers that need extra fields.
/// </summary>
public sealed class TbcItemSchema : DbcSchemaBase<ItemRecord>
{
    public override string FileName => "Item.dbc";

    private const int Entry              = 0;
    private const int ClassId            = 1;
    private const int SubclassId         = 2;
    private const int SoundOverrideSub   = 3;
    private const int Material           = 4;
    private const int DisplayId          = 5;
    private const int InventoryType      = 6;
    private const int SheatheType        = 7;

    public override ItemRecord ProjectTyped(DbcRecord raw) => new()
    {
        Entry                  = raw.GetInt32OrDefault(Entry),
        ClassId                = raw.GetInt32OrDefault(ClassId),
        SubclassId             = raw.GetInt32OrDefault(SubclassId),
        SoundOverrideSubclass  = raw.GetInt32OrDefault(SoundOverrideSub),
        Material               = raw.GetInt32OrDefault(Material),
        DisplayId              = raw.GetInt32OrDefault(DisplayId),
        InventoryType          = raw.GetInt32OrDefault(InventoryType),
        SheatheType            = raw.GetInt32OrDefault(SheatheType),
    };
}
