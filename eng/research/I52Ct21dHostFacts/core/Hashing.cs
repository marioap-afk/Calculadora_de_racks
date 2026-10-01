using System.Security.Cryptography;
using System.Text;

namespace I52Ct21d.HostFacts.Core;

/// <summary>SHA-256 helpers. Lowercase hexadecimal everywhere (the schemas pin <c>^[0-9a-f]{64}$</c>).</summary>
public static class Sha256Hex
{
    public static string Of(ReadOnlySpan<byte> data) => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

    public static string OfUtf8(string text) => Of(new UTF8Encoding(false, true).GetBytes(text));

    /// <summary>Hash of a file read through a read-only stream (never opens a file for write).</summary>
    public static string OfFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}

/// <summary>Binary64 bit patterns as 16 uppercase hexadecimal digits (the schemas pin <c>^[0-9A-F]{16}$</c>).</summary>
public static class DoubleBits
{
    public static string Hex(double value) => BitConverter.DoubleToUInt64Bits(value).ToString("X16");

    public static double FromHex(string hex)
    {
        if (hex.Length != 16) throw new FormatException("a binary64 pattern has exactly 16 hexadecimal digits");
        return BitConverter.UInt64BitsToDouble(Convert.ToUInt64(hex, 16));
    }
}
