namespace ParsWoW.Api.Infrastructure.Dbc.Engine;

/// <summary>
/// One physical row of a DBC file. Holds the raw uint32 values plus the
/// shared string block. Type disambiguation is delegated entirely to
/// per-expansion schemas (which know which column is int vs float vs
/// string-offset), so this type stays allocation-free: no boxes, no
/// per-field <c>object[]</c>.
/// </summary>
public sealed class DbcRecord
{
    private readonly uint[] _values;

    public DbcRecord(uint[] values, IReadOnlyDictionary<int, string> stringBlock)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = values;
        StringBlock = stringBlock ?? throw new ArgumentNullException(nameof(stringBlock));
    }

    public IReadOnlyDictionary<int, string> StringBlock { get; }
    public int FieldCount => _values.Length;

    /// <summary>Returns the uint32 at <paramref name="column"/>, or throws if out of bounds.</summary>
    public uint GetUInt32(int column) =>
        (uint)column < (uint)_values.Length
            ? _values[column]
            : throw new IndexOutOfRangeException(
                $"Column {column} out of range (FieldCount={_values.Length}).");

    /// <summary>Returns the int32 at <paramref name="column"/>, or throws if out of bounds.</summary>
    public int GetInt32(int column) => unchecked((int)GetUInt32(column));

    /// <summary>Returns the float at <paramref name="column"/>, or throws if out of bounds.</summary>
    public float GetFloat(int column) =>
        BitConverter.Int32BitsToSingle(unchecked((int)GetUInt32(column)));

    /// <summary>
    /// Reads the column as a string-block index. Schemas only call this
    /// when they know the column is a string reference.
    /// </summary>
    public string GetString(int column)
    {
        var offset = GetInt32(column);
        return StringBlock.TryGetValue(offset, out var s) ? s : string.Empty;
    }

    // ---------- Safe (default-value) overloads for repacks / stripped DBCs ----------

    /// <summary>Returns the uint32 at <paramref name="column"/>,
    /// or <paramref name="defaultValue"/> if the column is out of range.</summary>
    public uint GetUInt32OrDefault(int column, uint defaultValue = 0) =>
        (uint)column < (uint)_values.Length ? _values[column] : defaultValue;

    /// <summary>Returns the int32 at <paramref name="column"/>,
    /// or <paramref name="defaultValue"/> if the column is out of range.</summary>
    public int GetInt32OrDefault(int column, int defaultValue = 0) =>
        unchecked((int)GetUInt32OrDefault(column, unchecked((uint)defaultValue)));

    /// <summary>Returns the float at <paramref name="column"/>,
    /// or <paramref name="defaultValue"/> if the column is out of range.</summary>
    public float GetFloatOrDefault(int column, float defaultValue = 0f)
    {
        if ((uint)column >= (uint)_values.Length)
            return defaultValue;
        return BitConverter.Int32BitsToSingle(unchecked((int)_values[column]));
    }

    /// <summary>Reads the column as a string-block index and resolves the string,
    /// or returns <paramref name="defaultValue"/> if the column is out of range
    /// or the offset is not found in the string block.</summary>
    public string GetStringOrDefault(int column, string defaultValue = "")
    {
        if ((uint)column >= (uint)_values.Length)
            return defaultValue;
        var offset = unchecked((int)_values[column]);
        return StringBlock.TryGetValue(offset, out var s) ? s : defaultValue;
    }
}
