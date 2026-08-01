using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.TBC;

public sealed class TbcEnchantmentSchema : DbcSchemaBase<ItemEnchantmentRecord>
{
    public override string FileName => "SpellItemEnchantment.dbc";

    public override ItemEnchantmentRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0),
        Charges = raw.GetInt32OrDefault(1),
        EffectType = raw.GetInt32OrDefault(2),
        EffectSpellId = raw.GetInt32OrDefault(5),
        EffectAmount = raw.GetInt32OrDefault(8),
        Name = raw.GetStringOrDefault(11)
    };
}
