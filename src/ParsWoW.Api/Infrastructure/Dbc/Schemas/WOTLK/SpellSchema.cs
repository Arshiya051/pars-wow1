using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.WOTLK;

public sealed class WotlkSpellSchema : DbcSchemaBase<SpellRecord>
{
    public override string FileName => "Spell.dbc";

    public override SpellRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0),
        Category = raw.GetInt32OrDefault(1),
        Dispel = raw.GetInt32OrDefault(2),
        Mechanic = raw.GetInt32OrDefault(3),
        Attributes = raw.GetInt32OrDefault(4),
        AttributesEx = raw.GetInt32OrDefault(5),
        SchoolMask = raw.GetInt32OrDefault(94),
    };
}
