using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ERP.Shared.Helpers;

/// <summary>RFC 6238 TOTP (HMAC-SHA1, 30s step, 6 digits) with Base32 secrets.</summary>
public static class TotpHelper
{
    private const int StepSeconds = 30;
    private const int Digits = 6;
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string GenerateSecret(int byteLength = 20)
    {
        var bytes = RandomNumberGenerator.GetBytes(Math.Clamp(byteLength, 16, 64));
        return ToBase32(bytes);
    }

    public static string BuildOtpAuthUri(string issuer, string accountName, string secret)
    {
        var label = Uri.EscapeDataString($"{issuer}:{accountName}");
        var iss = Uri.EscapeDataString(issuer);
        return $"otpauth://totp/{label}?secret={secret}&issuer={iss}&algorithm=SHA1&digits={Digits}&period={StepSeconds}";
    }

    public static bool Verify(string base32Secret, string code, int window = 1, DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(base32Secret) || string.IsNullOrWhiteSpace(code))
            return false;
        if (!int.TryParse(code.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var provided))
            return false;

        var key = FromBase32(base32Secret);
        if (key.Length == 0) return false;

        var timestep = GetTimestep(utcNow ?? DateTime.UtcNow);
        for (var i = -window; i <= window; i++)
        {
            if (ComputeCode(key, timestep + i) == provided)
                return true;
        }
        return false;
    }

    public static string ComputeCode(string base32Secret, DateTime? utcNow = null)
    {
        var key = FromBase32(base32Secret);
        if (key.Length == 0) throw new ArgumentException("Invalid Base32 secret.", nameof(base32Secret));
        return ComputeCode(key, GetTimestep(utcNow ?? DateTime.UtcNow)).ToString($"D{Digits}", CultureInfo.InvariantCulture);
    }

    private static long GetTimestep(DateTime utcNow) =>
        (long)Math.Floor((utcNow.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds / StepSeconds);

    private static int ComputeCode(byte[] key, long timestep)
    {
        Span<byte> counter = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counter, timestep);
        var hash = HMACSHA1.HashData(key, counter);
        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24)
            | ((hash[offset + 1] & 0xFF) << 16)
            | ((hash[offset + 2] & 0xFF) << 8)
            | (hash[offset + 3] & 0xFF);
        return binary % (int)Math.Pow(10, Digits);
    }

    public static string ToBase32(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0) return string.Empty;
        var sb = new StringBuilder((data.Length * 8 + 4) / 5);
        var buffer = 0;
        var bitsLeft = 0;
        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                sb.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 0x1F]);
                bitsLeft -= 5;
            }
        }
        if (bitsLeft > 0)
            sb.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
        return sb.ToString();
    }

    public static byte[] FromBase32(string input)
    {
        var cleaned = input.Trim().TrimEnd('=').Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        if (cleaned.Length == 0) return Array.Empty<byte>();

        var output = new List<byte>((cleaned.Length * 5) / 8);
        var buffer = 0;
        var bitsLeft = 0;
        foreach (var c in cleaned)
        {
            var val = Base32Alphabet.IndexOf(c);
            if (val < 0) return Array.Empty<byte>();
            buffer = (buffer << 5) | val;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }
        return output.ToArray();
    }
}
