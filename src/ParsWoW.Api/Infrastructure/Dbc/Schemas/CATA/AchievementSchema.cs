using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.CATA;

public sealed class CataAchievementSchema : DbcSchemaBase<AchievementRecord>
{
    public override string FileName => "Achievement.dbc";
    public override AchievementRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), Faction = raw.GetInt32OrDefault(1), InstanceId = raw.GetInt32OrDefault(2),
        Title = raw.GetStringOrDefault(3), Description = raw.GetStringOrDefault(4),
        Category = raw.GetInt32OrDefault(5), Points = raw.GetInt32OrDefault(6), OrderInCategory = raw.GetInt32OrDefault(7)
    };
}
