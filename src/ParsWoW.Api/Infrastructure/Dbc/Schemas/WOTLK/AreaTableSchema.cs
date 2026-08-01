using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.WOTLK;

public sealed class WotlkAreaTableSchema : DbcSchemaBase<AreaRecord>
{
    public override string FileName => "AreaTable.dbc";
    public override AreaRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), ContinentId = raw.GetInt32OrDefault(1), ParentAreaId = raw.GetInt32OrDefault(2),
        Flags = raw.GetInt32OrDefault(3), AreaName = raw.GetStringOrDefault(4)
    };
}
