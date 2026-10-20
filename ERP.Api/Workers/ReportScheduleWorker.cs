using ERP.Application.Interfaces.Reports;

namespace ERP.Api.Workers;

/// <summary>Polls due report schedules and executes them via <see cref="IReportService"/>.</summary>
public sealed class ReportScheduleWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<ReportScheduleWorker> _logger;
    private readonly TimeSpan _interval;

    public ReportScheduleWorker(
        IServiceScopeFactory scopes,
        IConfiguration configuration,
        ILogger<ReportScheduleWorker> logger)
    {
        _scopes = scopes;
        _logger = logger;
        var seconds = configuration.GetValue("Reports:SchedulePollSeconds", 60);
        if (seconds < 5) seconds = 5;
        _interval = TimeSpan.FromSeconds(seconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = true;
        // Read once per loop so operators can disable without restart via reloadable config if wired.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                enabled = config.GetValue("Reports:ScheduleWorkerEnabled", true);
                if (enabled)
                {
                    var reports = scope.ServiceProvider.GetRequiredService<IReportService>();
                    var ran = await reports.ExecuteDueSchedulesAsync(stoppingToken);
                    if (ran > 0)
                        _logger.LogInformation("Report schedule worker executed {Count} due schedule(s).", ran);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Report schedule worker tick failed.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
