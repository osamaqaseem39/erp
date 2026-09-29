using ERP.Shared.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace ERP.Api.Security;

/// <summary>
/// ASP.NET Data Protection with a dedicated purpose string. The key ring location is configured in
/// Program.cs (DataProtection:KeysPath); losing it means re-entering stored secrets, not losing data.
/// </summary>
public sealed class DataProtectionSecretProtector : ISecretProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("ERP.Settings.Secrets.v1");
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}
