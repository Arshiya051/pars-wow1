using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.LEGION;

public sealed class LegionChrClassesSchema : DbcSchemaBase<ChrClassRecord>
{
    public override string FileName => "ChrClasses.dbc";
    public override ChrClassRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), Name = raw.GetStringOrDefault(1), PowerType = raw.GetInt32OrDefault(2)
    };
}
