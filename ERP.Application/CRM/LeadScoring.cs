using ERP.Domain.CRM;

namespace ERP.Application.CRM;

/// <summary>
/// Rule-based 0–100 lead score, recalculated whenever a lead or its activities change.
/// Completeness (35) + source quality (20) + priority (20) + deal size (15) + recent contact (10).
/// </summary>
public static class LeadScoring
{
    public static int Score(Lead lead, DateTime utcNow)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(lead.Email)) score += 10;
        if (!string.IsNullOrWhiteSpace(lead.Phone)) score += 10;
        if (!string.IsNullOrWhiteSpace(lead.CompanyName)) score += 10;
        if (!string.IsNullOrWhiteSpace(lead.Industry)) score += 5;

        score += (lead.LeadSource ?? "").ToLowerInvariant() switch
        {
            "referral" => 20,
            "website" or "event" => 15,
            "email campaign" or "social media" => 10,
            "cold call" or "other" => 5,
            _ => 0
        };

        score += (lead.Priority ?? "").ToLowerInvariant() switch
        {
            "urgent" => 20,
            "high" => 15,
            "medium" => 10,
            _ => 0
        };

        score += lead.EstimatedValue switch
        {
            >= 50_000 => 15,
            >= 10_000 => 10,
            > 0 => 5,
            _ => 0
        };

        if (lead.LastContactDate is { } contacted)
        {
            var days = (utcNow - contacted).TotalDays;
            if (days <= 14) score += 10;
            else if (days <= 30) score += 5;
        }

        return Math.Clamp(score, 0, 100);
    }
}
