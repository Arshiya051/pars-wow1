using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.WOTLK;

public sealed class WotlkTalentSchema : DbcSchemaBase<TalentRecord>
{
    public override string FileName => "Talent.dbc";
    public override TalentRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), TabId = raw.GetInt32OrDefault(1), TierId = raw.GetInt32OrDefault(2),
        ColumnIndex = raw.GetInt32OrDefault(3), SpellId = raw.GetInt32OrDefault(4), Ranks = raw.GetInt32OrDefault(5)
    };
}
