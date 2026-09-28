using ERP.Shared.Helpers;
using Xunit;

namespace ERP.Tests.Security;

public class PasswordHelperTests
{
    [Fact]
    public void HashPassword_ProducesSaltAndVerifies()
    {
        var (hash, salt) = PasswordHelper.HashPassword("SecurePass123!");
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.False(string.IsNullOrWhiteSpace(salt));
        Assert.True(PasswordHelper.VerifyPassword("SecurePass123!", hash, salt));
        Assert.False(PasswordHelper.VerifyPassword("wrong", hash, salt));
    }

    [Fact]
    public void VerifyPassword_SupportsLegacyUnsaltedSha256()
    {
        // Simulate legacy hash (SHA-256, no salt)
        using var sha = System.Security.Cryptography.SHA256.Create();
        var legacy = Convert.ToBase64String(
            sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("old-password")));

        Assert.True(PasswordHelper.VerifyPassword("old-password", legacy, salt: null));
        Assert.True(PasswordHelper.NeedsRehash(null));
        Assert.False(PasswordHelper.NeedsRehash("abc"));
    }

    [Fact]
    public void GenerateRefreshToken_IsHighEntropy()
    {
        var a = JwtHelper.GenerateRefreshToken();
        var b = JwtHelper.GenerateRefreshToken();
        Assert.NotEqual(a, b);
        Assert.True(a.Length >= 40);
    }
}
