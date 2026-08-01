using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Infrastructure.Dbc.Schemas.MOP;

public sealed class MopItemSetSchema : DbcSchemaBase<ItemSetRecord>
{
    public override string FileName => "ItemSet.dbc";
    public override ItemSetRecord ProjectTyped(DbcRecord raw) => new()
    {
        Id = raw.GetInt32OrDefault(0), Name = raw.GetStringOrDefault(1),
        ItemIds = new[] { raw.GetInt32OrDefault(2), raw.GetInt32OrDefault(3), raw.GetInt32OrDefault(4), raw.GetInt32OrDefault(5), raw.GetInt32OrDefault(6),
                          raw.GetInt32OrDefault(7), raw.GetInt32OrDefault(8), raw.GetInt32OrDefault(9), raw.GetInt32OrDefault(10), raw.GetInt32OrDefault(11),
                          raw.GetInt32OrDefault(12), raw.GetInt32OrDefault(13), raw.GetInt32OrDefault(14), raw.GetInt32OrDefault(15), raw.GetInt32OrDefault(16),
                          raw.GetInt32OrDefault(17), raw.GetInt32OrDefault(18) },
        SpellId = raw.GetInt32OrDefault(19)
    };
}
