namespace ERP.Application.Reports;

/// <summary>
/// Computes next run times from coarse ScheduleType values (Daily/Weekly/Monthly/…).
/// Full cron parsing is out of scope for v1 — CronExpression is stored for display/future use.
/// </summary>
public static class ReportScheduleCalculator
{
    public static DateTime ComputeNextRun(string? scheduleType, DateTime fromUtc)
    {
        var start = fromUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc)
            : fromUtc.ToUniversalTime();

        return (scheduleType ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "hourly" => start.AddHours(1),
            "daily" => start.AddDays(1),
            "weekly" => start.AddDays(7),
            "monthly" => start.AddMonths(1),
            "quarterly" => start.AddMonths(3),
            "yearly" or "annually" => start.AddYears(1),
            _ => start.AddDays(1)
        };
    }

    /// <summary>First run is due immediately; subsequent runs use <see cref="ComputeNextRun"/>.</summary>
    public static DateTime InitialNextRun(string? scheduleType, DateTime createdUtc)
        => createdUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(createdUtc, DateTimeKind.Utc)
            : createdUtc.ToUniversalTime();
}
