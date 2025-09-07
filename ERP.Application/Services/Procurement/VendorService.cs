using ERP.Application.Interfaces.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using ERP.Shared.Mappers.Procurement;

namespace ERP.Application.Services.Procurement;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly VendorMapper _mapper;

    public VendorService(IVendorRepository vendorRepository, VendorMapper mapper)
    {
        _vendorRepository = vendorRepository;
        _mapper = mapper;
    }

    // Additional methods needed by the controller
    public async Task<VendorDto> CreateAsync(CreateVendorDto dto, Guid tenantId, Guid userId)
    {
        var vendorCreateDto = new CreateVendorDto
        {
            VendorCode = dto.VendorCode,
            VendorName = dto.VendorName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            AddressLine1 = dto.AddressLine1,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            CompanyId = dto.CompanyId,
            PaymentTerms = dto.PaymentTerms,
            CreditLimit = dto.CreditLimit,
            IsActive = dto.IsActive
        };

        var vendorId = await AddVendorAsync(vendorCreateDto, tenantId, userId);
        return new VendorDto { Id = vendorId };
    }

    public async Task<VendorDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await GetVendorByIdAsync(id, tenantId);
    }

    public async Task<IEnumerable<VendorDto>> GetAllAsync(Guid tenantId, string? searchTerm = null, string? category = null, bool? isActive = null)
    {
        var query = new VendorQuery
        {
            TenantId = tenantId,
            SearchTerm = searchTerm,
            IsActive = isActive,
            PageNumber = 1,
            PageSize = int.MaxValue
        };

        var (vendors, _) = await SearchVendorsAsync(query);
        return vendors;
    }

    public async Task<VendorDto> UpdateAsync(UpdateVendorDto dto, Guid tenantId, Guid userId)
    {
        var vendorUpdateDto = new UpdateVendorDto
        {
            Id = dto.Id,
            VendorCode = dto.VendorCode,
            VendorName = dto.VendorName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            AddressLine1 = dto.AddressLine1,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            PaymentTerms = dto.PaymentTerms,
            CreditLimit = dto.CreditLimit,
            IsActive = dto.IsActive
        };

        await UpdateVendorAsync(vendorUpdateDto, tenantId, userId);
        return new VendorDto { Id = dto.Id };
    }

    public async Task<VendorDto> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id, tenantId);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID '{id}' not found.");

        // Add approval logic here
        return new VendorDto { Id = id };
    }

    public async Task<VendorDto> SuspendAsync(Guid id, SuspendVendorDto dto, Guid tenantId, Guid userId)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id, tenantId);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID '{id}' not found.");

        // Add suspension logic here
        return new VendorDto { Id = id };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await DeleteVendorAsync(id, tenantId);
    }

    public async Task<Guid> AddVendorAsync(CreateVendorDto dto, Guid tenantId, Guid userId)
    {
        // Validate vendor code uniqueness
        if (await _vendorRepository.ExistsByCodeAsync(dto.VendorCode, dto.CompanyId, tenantId))
        {
            throw new InvalidOperationException($"Vendor code '{dto.VendorCode}' already exists for this company.");
        }

        // Validate email uniqueness
        if (!string.IsNullOrEmpty(dto.Email) && await _vendorRepository.ExistsByEmailAsync(dto.Email, tenantId))
        {
            throw new InvalidOperationException($"Vendor email '{dto.Email}' already exists.");
        }

        var vendor = _mapper.MapToEntity(dto, tenantId, userId.ToString());
        return await _vendorRepository.AddAsync(vendor);
    }

    public async Task<Guid> UpdateVendorAsync(UpdateVendorDto dto, Guid tenantId, Guid userId)
    {
        var existingVendor = await _vendorRepository.GetByIdAsync(dto.Id, tenantId);
        if (existingVendor == null)
        {
            throw new KeyNotFoundException($"Vendor with ID '{dto.Id}' not found.");
        }

        // Validate vendor code uniqueness (excluding current vendor)
        if (await _vendorRepository.ExistsByCodeAsync(dto.VendorCode, existingVendor.CompanyId, tenantId, dto.Id))
        {
            throw new InvalidOperationException($"Vendor code '{dto.VendorCode}' already exists for this company.");
        }

        // Validate email uniqueness (excluding current vendor)
        if (!string.IsNullOrEmpty(dto.Email) && await _vendorRepository.ExistsByEmailAsync(dto.Email, tenantId, dto.Id))
        {
            throw new InvalidOperationException($"Vendor email '{dto.Email}' already exists.");
        }

        _mapper.MapToEntity(dto, existingVendor, userId.ToString());
        await _vendorRepository.UpdateAsync(existingVendor);
        return dto.Id;
    }

    public async Task<bool> DeleteVendorAsync(Guid vendorId, Guid tenantId)
    {
        if (!await _vendorRepository.ExistsAsync(vendorId, tenantId))
        {
            throw new KeyNotFoundException($"Vendor with ID '{vendorId}' not found.");
        }

        return await _vendorRepository.DeleteAsync(vendorId, tenantId);
    }

    public async Task<VendorDto?> GetVendorByIdAsync(Guid vendorId, Guid tenantId)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, tenantId);
        return vendor != null ? _mapper.MapToDto(vendor) : null;
    }

    public async Task<(IEnumerable<VendorDto> Vendors, int TotalCount)> SearchVendorsAsync(VendorQuery query)
    {
        var (vendors, totalCount) = await _vendorRepository.SearchAsync(
            query.TenantId,
            query.CompanyId,
            query.SearchTerm,
            query.IsActive,
            query.PageNumber,
            query.PageSize,
            query.SortBy,
            query.SortDirection);

        var vendorDtos = vendors.Select(_mapper.MapToDto);
        return (vendorDtos, totalCount);
    }

    public async Task<Guid> AddVendorBankAccountAsync(CreateVendorBankAccountDto dto, Guid vendorId, Guid tenantId, Guid userId)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, tenantId);
        if (vendor == null)
        {
            throw new KeyNotFoundException($"Vendor with ID '{vendorId}' not found.");
        }

        var bankAccount = _mapper.MapToBankAccountEntity(dto, vendorId, tenantId, userId.ToString());
        // Implementation for adding bank account would go here
        return Guid.NewGuid(); // Placeholder
    }

    public async Task<Guid> UpdateVendorBankAccountAsync(UpdateVendorBankAccountDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for updating bank account would go here
        return dto.Id;
    }

    public async Task<bool> DeleteVendorBankAccountAsync(Guid bankAccountId, Guid tenantId)
    {
        // Implementation for deleting bank account would go here
        return true;
    }

    public async Task<IEnumerable<VendorBankAccountDto>> GetVendorBankAccountsAsync(Guid vendorId, Guid tenantId)
    {
        // Implementation for getting bank accounts would go here
        return new List<VendorBankAccountDto>();
    }

    public async Task<bool> ValidateVendorCodeAsync(string vendorCode, Guid companyId, Guid tenantId, Guid? excludeVendorId = null)
    {
        return !await _vendorRepository.ExistsByCodeAsync(vendorCode, companyId, tenantId, excludeVendorId);
    }

    public async Task<bool> ValidateVendorEmailAsync(string email, Guid tenantId, Guid? excludeVendorId = null)
    {
        return !await _vendorRepository.ExistsByEmailAsync(email, tenantId, excludeVendorId);
    }
}