using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.CATA;

public sealed class CataGemPropertiesSchema : DbcSchemaBase<GemPropertiesRecord>
{
    public override string FileName => "GemProperties.dbc";
    public override GemPropertiesRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), SpellItemEnchantment = raw.GetInt32OrDefault(1),
        MaxCount = raw.GetInt32OrDefault(2), MinLevel = raw.GetInt32OrDefault(3)
    };
}
