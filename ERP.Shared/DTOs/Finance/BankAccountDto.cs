using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance
{
    public class BankAccountDto : BaseDto
    {
        public Guid? BankAccountId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid BankId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? CurrencyCode { get; set; }
        public decimal Balance { get; set; }
        public string? IBAN { get; set; }
        public string? SWIFT { get; set; }
        public Guid GLAccountId { get; set; }
        public string? GLAccountCode { get; set; }
        public string? GLAccountName { get; set; }
        public new bool IsActive { get; set; }
        public string BankName { get; set; } = string.Empty;
    }

    public class CreateBankAccountDto : CreateDto
    {
        public Guid CompanyId { get; set; }
        public Guid BankId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string? IBAN { get; set; }
        public string? SWIFT { get; set; }
        public Guid GLAccountId { get; set; }
        public new bool IsActive { get; set; } = true;
    }

    public class UpdateBankAccountDto : UpdateDto
    {
        public Guid BankId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string? IBAN { get; set; }
        public string? SWIFT { get; set; }
        public Guid GLAccountId { get; set; }
        public new bool IsActive { get; set; }
    }
}