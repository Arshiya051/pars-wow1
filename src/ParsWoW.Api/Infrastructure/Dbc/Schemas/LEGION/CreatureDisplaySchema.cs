using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.LEGION;

public sealed class LegionCreatureDisplaySchema : DbcSchemaBase<CreatureDisplayInfoRecord>
{
    public override string FileName => "CreatureDisplayInfo.dbc";
    public override CreatureDisplayInfoRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), ModelId = raw.GetInt32OrDefault(1),
        Texture1 = raw.GetStringOrDefault(2), Texture2 = raw.GetStringOrDefault(3), Scale = raw.GetFloatOrDefault(4)
    };
}
