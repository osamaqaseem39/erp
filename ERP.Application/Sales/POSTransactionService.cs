using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Sales;

public class POSTransactionService : IPOSTransactionService
{
    private readonly IPOSTransactionRepository _repository;
    private readonly IPOSTransactionLineRepository _lineRepository;

    public POSTransactionService(
        IPOSTransactionRepository repository,
        IPOSTransactionLineRepository lineRepository)
    {
        _repository = repository;
        _lineRepository = lineRepository;
    }

    public async Task<POSTransactionDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return null;

        var dto = MapToDto(entity);
        var lines = await _lineRepository.GetByTransactionIdAsync(id, tenantId);
        dto.Lines = lines.Select(MapLineToDto).ToList();
        return dto;
    }

    public async Task<IEnumerable<POSTransactionDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreatePOSTransactionDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        ApplyLineTotals(entity, dto.Lines);
        entity.ChangeAmount = entity.AmountPaid - entity.TotalAmount;

        var created = await _repository.AddAsync(entity);
        await SaveLinesAsync(created.Id, tenantId, userId, dto.Lines);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdatePOSTransactionDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity, userId);
        ApplyLineTotals(entity, dto.Lines.Select(ToCreateLine));
        entity.ChangeAmount = entity.AmountPaid - entity.TotalAmount;

        await _lineRepository.DeleteByTransactionIdAsync(entity.Id, tenantId);
        await SaveLinesAsync(entity.Id, tenantId, userId, dto.Lines.Select(ToCreateLine));
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;

        await _lineRepository.DeleteByTransactionIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetBySessionAsync(Guid sessionId, Guid tenantId)
    {
        var entities = await _repository.GetBySessionAsync(sessionId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);
        return entities.Select(MapToDto);
    }

    private async Task SaveLinesAsync(
        Guid transactionId,
        Guid tenantId,
        Guid userId,
        IEnumerable<CreatePOSTransactionLineDto> lines)
    {
        var index = 1;
        foreach (var lineDto in lines)
        {
            var computed = ComputeLineAmounts(lineDto.Quantity, lineDto.UnitPrice, lineDto.DiscountRate, lineDto.TaxRate);
            var line = new POSTransactionLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                POSTransactionId = transactionId,
                ItemId = lineDto.ItemId,
                Description = lineDto.Description ?? string.Empty,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountRate = lineDto.DiscountRate,
                DiscountAmount = computed.DiscountAmount,
                TaxRate = lineDto.TaxRate,
                TaxAmount = computed.TaxAmount,
                LineTotal = computed.LineTotal,
                LineNumber = lineDto.LineNumber > 0 ? lineDto.LineNumber : index,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _lineRepository.CreateAsync(line);
            index++;
        }
    }

    private static void ApplyLineTotals(POSTransaction entity, IEnumerable<CreatePOSTransactionLineDto> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0) return;

        decimal subTotal = 0, discountAmount = 0, taxAmount = 0;
        foreach (var line in lineList)
        {
            var computed = ComputeLineAmounts(line.Quantity, line.UnitPrice, line.DiscountRate, line.TaxRate);
            subTotal += computed.Gross;
            discountAmount += computed.DiscountAmount;
            taxAmount += computed.TaxAmount;
        }

        entity.SubTotal = Math.Round(subTotal, 2);
        entity.DiscountAmount = Math.Round(discountAmount, 2);
        entity.TaxAmount = Math.Round(taxAmount, 2);
        entity.TotalAmount = Math.Round(subTotal - discountAmount + taxAmount, 2);
    }

    private static (decimal Gross, decimal DiscountAmount, decimal TaxAmount, decimal LineTotal) ComputeLineAmounts(
        decimal quantity,
        decimal unitPrice,
        decimal discountRate,
        decimal taxRate)
    {
        var gross = Math.Round(quantity * unitPrice, 2);
        var discount = Math.Round(gross * discountRate / 100m, 2);
        var net = gross - discount;
        var tax = Math.Round(net * taxRate / 100m, 2);
        return (gross, discount, tax, net + tax);
    }

    private static CreatePOSTransactionLineDto ToCreateLine(UpdatePOSTransactionLineDto line) => new()
    {
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        DiscountRate = line.DiscountRate,
        TaxRate = line.TaxRate,
        LineNumber = line.LineNumber
    };

    private static POSTransactionDto MapToDto(POSTransaction entity) => new()
    {
        Id = entity.Id,
        TransactionNumber = entity.TransactionNumber,
        POSSessionId = entity.POSSessionId,
        SessionId = entity.SessionId,
        CustomerId = entity.CustomerId,
        TransactionDate = entity.TransactionDate,
        PaymentMethod = entity.PaymentMethod,
        SubTotal = entity.SubTotal,
        TaxAmount = entity.TaxAmount,
        DiscountAmount = entity.DiscountAmount,
        TotalAmount = entity.TotalAmount,
        AmountPaid = entity.AmountPaid,
        ChangeAmount = entity.ChangeAmount,
        Status = entity.Status,
        Notes = entity.Notes,
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static POSTransactionLineDto MapLineToDto(POSTransactionLine line) => new()
    {
        Id = line.Id,
        POSTransactionId = line.POSTransactionId,
        ItemId = line.ItemId,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        DiscountRate = line.DiscountRate,
        DiscountAmount = line.DiscountAmount,
        TaxRate = line.TaxRate,
        TaxAmount = line.TaxAmount,
        LineTotal = line.LineTotal,
        LineNumber = line.LineNumber
    };

    private static POSTransaction MapToEntity(CreatePOSTransactionDto dto, Guid tenantId, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        TransactionNumber = dto.TransactionNumber,
        POSSessionId = dto.POSSessionId,
        SessionId = dto.SessionId != Guid.Empty ? dto.SessionId : dto.POSSessionId,
        CustomerId = dto.CustomerId,
        TransactionDate = dto.TransactionDate == default ? DateTime.UtcNow : dto.TransactionDate,
        PaymentMethod = dto.PaymentMethod,
        SubTotal = dto.SubTotal,
        TaxAmount = dto.TaxAmount,
        DiscountAmount = dto.DiscountAmount,
        TotalAmount = dto.TotalAmount,
        AmountPaid = dto.AmountPaid,
        ChangeAmount = dto.AmountPaid - dto.TotalAmount,
        Status = string.IsNullOrWhiteSpace(dto.Status) ? "Completed" : dto.Status,
        Notes = dto.Notes,
        CreatedBy = userId.ToString(),
        CreatedAt = DateTime.UtcNow
    };

    private static void MapToEntity(UpdatePOSTransactionDto dto, POSTransaction entity, Guid userId)
    {
        entity.TransactionNumber = dto.TransactionNumber;
        entity.SessionId = dto.SessionId;
        entity.CustomerId = dto.CustomerId;
        entity.TransactionDate = dto.TransactionDate;
        entity.PaymentMethod = dto.PaymentMethod;
        entity.SubTotal = dto.SubTotal;
        entity.TaxAmount = dto.TaxAmount;
        entity.DiscountAmount = dto.DiscountAmount;
        entity.TotalAmount = dto.TotalAmount;
        entity.AmountPaid = dto.AmountPaid;
        entity.ChangeAmount = dto.AmountPaid - dto.TotalAmount;
        // ERP-FIX-5: status is NOT free-set from dto; lifecycle endpoints govern it.
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
