using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.LEGION;

public sealed class LegionChrRacesSchema : DbcSchemaBase<ChrRaceRecord>
{
    public override string FileName => "ChrRaces.dbc";
    public override ChrRaceRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), Name = raw.GetStringOrDefault(1),
        FactionId = raw.GetInt32OrDefault(2), Flags = raw.GetInt32OrDefault(3)
    };
}
