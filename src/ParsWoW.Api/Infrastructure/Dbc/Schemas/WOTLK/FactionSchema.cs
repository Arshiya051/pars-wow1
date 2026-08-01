using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.WOTLK;

public sealed class WotlkFactionSchema : DbcSchemaBase<FactionRecord>
{
    public override string FileName => "Faction.dbc";
    public override FactionRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), ReputationRaceMask = raw.GetInt32OrDefault(1),
        Name = raw.GetStringOrDefault(2), Description = raw.GetStringOrDefault(3), Flags = raw.GetInt32OrDefault(4)
    };
}
