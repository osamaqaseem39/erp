namespace ERP.Shared.Abstractions;

/// <summary>Encrypts values stored at rest (sensitive settings). Decrypted values are for server use only.</summary>
public interface ISecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}
