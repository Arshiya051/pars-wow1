using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.WOTLK;

public sealed class WotlkItemDisplaySchema : DbcSchemaBase<ItemDisplayInfoRecord>
{
    public override string FileName => "ItemDisplayInfo.dbc";
    public override ItemDisplayInfoRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), Model1 = raw.GetStringOrDefault(1), Model2 = raw.GetStringOrDefault(2),
        Texture = raw.GetStringOrDefault(3), GeosetGroup = raw.GetInt32OrDefault(4)
    };
}
