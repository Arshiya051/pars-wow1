using System.Globalization;
using System.Numerics;

namespace ParsWoW.Api.Infrastructure.Auth;

/// <summary>
/// Big-Endian hex utilities used by the WoW SRP6 password verifiers.
/// BlizzCMS / WoW emulators work in big-endian byte order and require
/// explicit reversal when converting between hex strings and BigInteger.
/// </summary>
internal static class BigIntegerExtensions
{
    /// <summary>Convert a hex string to a <see cref="BigInteger"/> (treats input as big-endian).</summary>
    public static BigInteger HexToBigInteger(string hex)
    {
        // BigInteger.Parse with AllowHexSpecifier expects an optional leading minus
        // for negative numbers.  SRP6 values are always positive.
        hex = hex.TrimStart('0');
        if (hex.Length == 0) hex = "0";
        return BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier);
    }

    /// <summary>Convert a <see cref="BigInteger"/> to an uppercase hex string, normalized to 32 bytes (64 chars).</summary>
    public static string BigIntegerToHex(BigInteger value)
    {
        byte[] bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);

        // SRP6 verifier must be exactly 32 bytes (64 hex chars)
        if (bytes.Length > 32)
            throw new InvalidOperationException($"Verifier exceeds 32 bytes: {bytes.Length}");

        if (bytes.Length < 32)
        {
            int oldLen = bytes.Length;
            Array.Resize(ref bytes, 32);
            Array.Fill<byte>(bytes, 0, oldLen, 32 - oldLen);
        }

        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Reverse the byte order of a hex string (each pair of hex chars = one byte).
    /// WoW SRP6 stores the hash in little-endian when computing BigInteger x,
    /// then reverses back for storage.
    /// </summary>
    public static string ReverseHex(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
            return hex;

        char[] chars = hex.ToCharArray();
        for (int i = 0; i < chars.Length; i += 2)
        {
            // Swap pairs from front to back
            int j = chars.Length - 2 - i;
            if (j <= i) break;
            (chars[i], chars[j]) = (chars[j], chars[i]);
            (chars[i + 1], chars[j + 1]) = (chars[j + 1], chars[i + 1]);
        }
        return new string(chars);
    }

    /// <summary>Concatenate two byte arrays.</summary>
    public static byte[] Concat(byte[] a, byte[] b)
    {
        var result = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, result, 0, a.Length);
        Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
        return result;
    }
}
