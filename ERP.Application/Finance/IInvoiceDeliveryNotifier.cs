using Microsoft.Extensions.Logging;

namespace ERP.Application.Finance;

/// <summary>
/// Delivers an AR invoice to the customer (email/PDF). Production wires SMTP/provider here;
/// host default logs until SMTP is configured; tests use <see cref="CapturingInvoiceDeliveryNotifier"/>.
/// </summary>
public interface IInvoiceDeliveryNotifier
{
    Task NotifyAsync(InvoiceDeliveryRequest request, CancellationToken ct = default);
}

public sealed record InvoiceDeliveryRequest(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid TenantId,
    Guid CustomerId,
    string RecipientEmail,
    decimal TotalAmount,
    string CurrencyCode,
    DateTime InvoiceDate,
    DateTime DueDate);

/// <summary>No-op delivery — service still updates invoice status when applicable.</summary>
public sealed class NullInvoiceDeliveryNotifier : IInvoiceDeliveryNotifier
{
    public Task NotifyAsync(InvoiceDeliveryRequest request, CancellationToken ct = default)
        => Task.CompletedTask;
}

/// <summary>Default host registration: log that a send was requested (no SMTP yet).</summary>
public sealed class LoggingInvoiceDeliveryNotifier : IInvoiceDeliveryNotifier
{
    private readonly ILogger<LoggingInvoiceDeliveryNotifier> _logger;

    public LoggingInvoiceDeliveryNotifier(ILogger<LoggingInvoiceDeliveryNotifier> logger) => _logger = logger;

    public Task NotifyAsync(InvoiceDeliveryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Invoice {InvoiceNumber} ({InvoiceId}) send requested to {Email} for tenant {TenantId}; amount {Amount} {Currency}. Wire SMTP for delivery.",
            request.InvoiceNumber,
            request.InvoiceId,
            request.RecipientEmail,
            request.TenantId,
            request.TotalAmount,
            request.CurrencyCode);
        return Task.CompletedTask;
    }
}

/// <summary>Test double that retains the last delivery request.</summary>
public sealed class CapturingInvoiceDeliveryNotifier : IInvoiceDeliveryNotifier
{
    public InvoiceDeliveryRequest? Last { get; private set; }

    public void Clear() => Last = null;

    public Task NotifyAsync(InvoiceDeliveryRequest request, CancellationToken ct = default)
    {
        Last = request;
        return Task.CompletedTask;
    }
}
