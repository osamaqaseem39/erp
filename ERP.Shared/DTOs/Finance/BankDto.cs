using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance
{
    public class BankDto : BaseDto
    {
        public Guid CompanyId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public new bool IsActive { get; set; }
    }

    public class CreateBankDto : CreateDto
    {
        public Guid CompanyId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public new bool IsActive { get; set; } = true;
    }

    public class UpdateBankDto : UpdateDto
    {
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public new bool IsActive { get; set; }
    }
}