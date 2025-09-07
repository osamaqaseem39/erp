using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Shared.Mappers.CRM;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            CustomerCode = entity.CustomerCode,
            CustomerName = entity.CustomerName,
            CustomerType = entity.CustomerType,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            Status = entity.Status,
            CreditLimit = entity.CreditLimit,
            OutstandingBalance = entity.OutstandingBalance,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static Customer ToEntity(this CustomerDto dto)
    {
        return new Customer
        {
            Id = dto.Id,
            CustomerCode = dto.CustomerCode,
            CustomerName = dto.CustomerName,
            CustomerType = dto.CustomerType,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            Status = dto.Status,
            CreditLimit = dto.CreditLimit,
            OutstandingBalance = dto.OutstandingBalance,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}