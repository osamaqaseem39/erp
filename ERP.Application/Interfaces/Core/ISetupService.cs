using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface ISetupService
{
    Task<SuperAdminSetupResponseDto> SetupSuperAdminAsync(SuperAdminSetupDto dto);
    Task<bool> IsSuperAdminExistsAsync();
}

