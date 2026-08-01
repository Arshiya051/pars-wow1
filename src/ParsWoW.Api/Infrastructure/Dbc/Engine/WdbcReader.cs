using System.Buffers.Binary;
using System.Text;

namespace ParsWoW.Api.Infrastructure.Dbc.Engine;

/// <summary>
/// Raw, expansion-agnostic WDBC/WDB binary reader. Parses the file
/// header, walks each record's column buffer, and reads the trailing
/// string block. Knows nothing about column meaning — schema
/// interpretation is delegated to a per-expansion <c>IDbcSchema</c>.
///
/// Per performance: we read sequentially from the supplied
/// <see cref="Stream"/> and store only <c>uint[]</c> rows so we don't
/// allocate a boxed per-field object array on the LOH.
/// </summary>
public sealed class WdbcReader
{
    public DbcFile Read(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        using var br = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);

        var header = ReadHeader(br);
        if (!header.IsValid)
            throw new InvalidDataException(
                $"DBC magic 0x{header.Magic:X8} is not a supported WDBC/WDB family.");

        long dataStart = 20;
        long dataEnd = dataStart + (long)header.RecordCount * header.RecordSize;
        long stringStart = dataEnd;
        long stringEnd = stringStart + header.StringBlockSize;

        if (dataEnd > input.Length && input.CanSeek)
            throw new InvalidDataException(
                $"DBC declares {header.RecordCount} records of {header.RecordSize} bytes but the file is only {input.Length} bytes.");

        var stringBlock = ReadStringBlock(br, (int)stringStart, (int)(stringEnd - stringStart));

        br.BaseStream.Position = dataStart;
        var records = new List<DbcRecord>(header.RecordCount);
        uint[] buffer = new uint[header.FieldCount];

        for (int r = 0; r < header.RecordCount; r++)
        {
            for (int c = 0; c < header.FieldCount; c++)
                buffer[c] = br.ReadUInt32();

            // Each row gets its own array because the string pool is shared,
            // but projecting schemas never mutate it so a rented buffer would
            // also work. We allocate per-row to keep the public surface simple.
            var rowValues = new uint[header.FieldCount];
            Array.Copy(buffer, rowValues, header.FieldCount);
            records.Add(new DbcRecord(rowValues, stringBlock));
        }

        return new DbcFile(header, records, stringBlock);
    }

    private static WdbcHeader ReadHeader(BinaryReader br)
    {
        return new WdbcHeader(
            Magic: br.ReadUInt32(),
            RecordCount: br.ReadInt32(),
            FieldCount: br.ReadInt32(),
            RecordSize: br.ReadInt32(),
            StringBlockSize: br.ReadInt32());
    }

    private static Dictionary<int, string> ReadStringBlock(BinaryReader br, int offset, int size)
    {
        var dict = new Dictionary<int, string>();
        if (size <= 0) return dict;

        br.BaseStream.Position = offset;
        var bytes = br.ReadBytes(size);
        var sb = new StringBuilder();
        int idx = 0;
        int local = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            if (b == 0)
            {
                dict[local] = sb.ToString();
                sb.Clear();
                local = i + 1;
                idx = i + 1;
            }
            else
            {
                sb.Append((char)b);
            }
        }

        // Compatibility shim: keep idx aligned with the last byte seen.
        _ = idx;
        return dict;
    }
}
