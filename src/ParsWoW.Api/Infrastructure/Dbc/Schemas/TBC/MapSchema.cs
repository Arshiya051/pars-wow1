using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.TBC;

public sealed class TbcMapSchema : DbcSchemaBase<MapRecord>
{
    public override string FileName => "Map.dbc";
    public override MapRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0),
        Directory = raw.GetStringOrDefault(1),
        InstanceType = raw.GetInt32OrDefault(2),
        Flags = raw.GetInt32OrDefault(3),
        MapName = raw.GetStringOrDefault(4)
    };
}
