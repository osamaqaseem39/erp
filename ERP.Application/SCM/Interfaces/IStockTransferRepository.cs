using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IStockTransferRepository
{
    Task<StockTransfer?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockTransfer>> GetAllAsync(Guid tenantId, Guid companyId);
    Task<StockTransfer> AddAsync(StockTransfer stockTransfer);
    Task<StockTransfer> UpdateAsync(StockTransfer stockTransfer);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ExistsAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockTransferDetail>> GetTransferDetailsAsync(Guid transferId);
    Task<StockTransferDetail> AddTransferDetailAsync(StockTransferDetail detail);
    Task<bool> UpdateTransferStatusAsync(Guid id, Guid tenantId, string status, string? approvedBy, string updatedBy);
}