namespace ParsWoW.Api.Infrastructure.Dbc.Engine;

/// <summary>
/// In-memory representation of one DBC file: header, raw records, and the
/// shared string table. Built once at startup and held by IDbcProvider
/// implementations; never re-read after warmup.
/// </summary>
public sealed class DbcFile
{
    public DbcFile(WdbcHeader header, IReadOnlyList<DbcRecord> records, IReadOnlyDictionary<int, string> stringBlock)
    {
        Header = header;
        Records = records;
        StringBlock = stringBlock;
    }

    public WdbcHeader Header { get; }
    public IReadOnlyList<DbcRecord> Records { get; }
    public IReadOnlyDictionary<int, string> StringBlock { get; }

    public int RecordCount => Records.Count;
    public int FieldCount => Header.FieldCount;
}
