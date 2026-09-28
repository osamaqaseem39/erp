using System.Security.Cryptography;
using System.Text;

namespace ERP.Shared.Helpers;

/// <summary>
/// PBKDF2 password hashing with per-user salt.
/// Legacy unsalted SHA-256 hashes (empty salt) are still verifiable and should be rehashed on login.
/// </summary>
public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            Algorithm,
            KeySize);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    /// <summary>
    /// Verifies a password. Supports legacy unsalted SHA-256 when <paramref name="salt"/> is null/empty.
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword, string? salt = null)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return false;

        // Legacy: unsalted SHA-256 (pre-hardening)
        if (string.IsNullOrEmpty(salt))
        {
            var legacyHash = HashPasswordLegacy(password);
            return string.Equals(legacyHash, hashedPassword, StringComparison.Ordinal);
        }

        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashBytes = Convert.FromBase64String(hashedPassword);
            var computed = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                Iterations,
                Algorithm,
                hashBytes.Length);
            return CryptographicOperations.FixedTimeEquals(computed, hashBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool NeedsRehash(string? salt) => string.IsNullOrEmpty(salt);

    public static string GenerateRandomPassword(int length = 12)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var result = new char[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        return new string(result);
    }

    private static string HashPasswordLegacy(string password)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
