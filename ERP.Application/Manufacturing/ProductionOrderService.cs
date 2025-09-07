using ERP.Application.Interfaces.Manufacturing;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Domain.Manufacturing;

namespace ERP.Application.Manufacturing;

public class ProductionOrderService : IProductionOrderService
{
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionOrderLineRepository _productionOrderLineRepository;
    private readonly IProductionRunRepository _productionRunRepository;
    private readonly IWorkCenterRepository _workCenterRepository;

    public ProductionOrderService(
        IProductionOrderRepository productionOrderRepository,
        IProductionOrderLineRepository productionOrderLineRepository,
        IProductionRunRepository productionRunRepository,
        IWorkCenterRepository workCenterRepository)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionOrderLineRepository = productionOrderLineRepository;
        _productionRunRepository = productionRunRepository;
        _workCenterRepository = workCenterRepository;
    }

    public async Task<ProductionOrderDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id);
        return productionOrder != null ? MapToDto(productionOrder) : null;
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetAllAsync(Guid tenantId, string? status = null, Guid? workCenterId = null)
    {
        var productionOrders = await _productionOrderRepository.GetAllAsync();
        
        if (status != null)
            productionOrders = productionOrders.Where(po => po.Status == status);
        if (workCenterId.HasValue)
            productionOrders = productionOrders.Where(po => po.WorkCenterId == workCenterId.Value);

        return productionOrders.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var productionOrders = await _productionOrderRepository.GetByStatusAsync(status);
        return productionOrders.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetByWorkCenterAsync(Guid workCenterId, Guid tenantId)
    {
        var productionOrders = await _productionOrderRepository.GetByWorkCenterAsync(workCenterId);
        return productionOrders.Select(MapToDto);
    }

    public async Task<ProductionOrderDto> CreateAsync(CreateProductionOrderDto dto, Guid tenantId, Guid userId)
    {
        var productionOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = dto.OrderNumber ?? Guid.NewGuid().ToString(),
            ItemId = dto.ItemId,
            Quantity = dto.Quantity ?? 0m,
            WorkCenterId = dto.WorkCenterId,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            Status = "Draft",
            Priority = dto.Priority ?? "Medium",
            Notes = dto.Notes,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _productionOrderRepository.CreateAsync(productionOrder);
        var created = await _productionOrderRepository.GetByIdAsync(createdId);
        return MapToDto(created!);
    }

    public async Task<ProductionOrderDto> UpdateAsync(UpdateProductionOrderDto dto, Guid tenantId, Guid userId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(dto.Id);
        if (productionOrder == null)
            throw new KeyNotFoundException($"Production Order with ID '{dto.Id}' not found.");

        productionOrder.OrderNumber = dto.OrderNumber ?? productionOrder.OrderNumber;
        productionOrder.ItemId = dto.ItemId;
        productionOrder.Quantity = dto.Quantity ?? productionOrder.Quantity;
        productionOrder.WorkCenterId = dto.WorkCenterId;
        productionOrder.PlannedStartDate = dto.PlannedStartDate;
        productionOrder.PlannedEndDate = dto.PlannedEndDate;
        productionOrder.Status = dto.Status ?? productionOrder.Status;
        productionOrder.Priority = dto.Priority ?? productionOrder.Priority;
        productionOrder.Notes = dto.Notes;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        return MapToDto(productionOrder);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _productionOrderRepository.DeleteAsync(id);
    }

    public async Task<ProductionOrderDto> StartAsync(Guid id, Guid tenantId, Guid userId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id);
        if (productionOrder == null)
            throw new KeyNotFoundException($"Production Order with ID '{id}' not found.");

        if (productionOrder.Status != "Draft")
            throw new InvalidOperationException("Only draft production orders can be started.");

        productionOrder.Status = "In Progress";
        productionOrder.ActualStartDate = DateTime.UtcNow;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        return MapToDto(productionOrder);
    }

    public async Task<ProductionOrderDto> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id);
        if (productionOrder == null)
            throw new KeyNotFoundException($"Production Order with ID '{id}' not found.");

        if (productionOrder.Status != "In Progress")
            throw new InvalidOperationException("Only in-progress production orders can be completed.");

        productionOrder.Status = "Completed";
        productionOrder.ActualEndDate = DateTime.UtcNow;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        return MapToDto(productionOrder);
    }

    public async Task<ProductionOrderDto> CancelAsync(Guid id, string reason, Guid tenantId, Guid userId)
    {
        var productionOrder = await _productionOrderRepository.GetByIdAsync(id);
        if (productionOrder == null)
            throw new KeyNotFoundException($"Production Order with ID '{id}' not found.");

        if (productionOrder.Status == "Completed" || productionOrder.Status == "Cancelled")
            throw new InvalidOperationException("Cannot cancel completed or already cancelled production orders.");

        productionOrder.Status = "Cancelled";
        productionOrder.CancellationReason = reason ?? string.Empty;
        productionOrder.UpdatedBy = userId.ToString();
        productionOrder.UpdatedAt = DateTime.UtcNow;

        await _productionOrderRepository.UpdateAsync(productionOrder);
        return MapToDto(productionOrder);
    }

    private static ProductionOrderDto MapToDto(ProductionOrder productionOrder)
    {
        return new ProductionOrderDto
        {
            Id = productionOrder.Id,
            OrderNumber = productionOrder.OrderNumber,
            ItemId = productionOrder.ItemId,
            Quantity = productionOrder.Quantity,
            WorkCenterId = productionOrder.WorkCenterId,
            PlannedStartDate = productionOrder.PlannedStartDate,
            PlannedEndDate = productionOrder.PlannedEndDate,
            ActualStartDate = productionOrder.ActualStartDate,
            ActualEndDate = productionOrder.ActualEndDate,
            Status = productionOrder.Status,
            Priority = productionOrder.Priority,
            Notes = productionOrder.Notes,
            CancellationReason = productionOrder.CancellationReason,
            TenantId = productionOrder.TenantId,
            CreatedBy = productionOrder.CreatedBy,
            CreatedAt = productionOrder.CreatedAt,
            UpdatedBy = productionOrder.UpdatedBy,
            UpdatedAt = productionOrder.UpdatedAt
        };
    }
}