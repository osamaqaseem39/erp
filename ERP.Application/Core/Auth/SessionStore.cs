using System.Security.Cryptography;
using System.Text;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Shared.Abstractions;
using ERP.Shared.Exceptions;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Core.Auth;

public sealed record IssuedRefreshToken(string Token, RefreshToken Row);

public sealed record RotatedRefreshToken(string Token, RefreshToken Row, User User, SecurityPolicy Policy);

public interface ISessionStore
{
    /// <summary>Starts a new sign-in session for the user and returns its first refresh token.</summary>
    Task<IssuedRefreshToken> StartAsync(User user, SecurityPolicy policy);

    /// <summary>Exchanges a refresh token for a new one in the same session.</summary>
    Task<RotatedRefreshToken> RotateAsync(string token);

    /// <summary>Ends the session the token belongs to. Unknown tokens are ignored.</summary>
    Task EndAsync(string token, string reason);

    /// <summary>Ends every open session of the user; returns how many there were.</summary>
    Task<int> EndAllAsync(Guid userId, string reason);
}

/// <summary>
/// Refresh tokens stored as hashes, one row per token, grouped by session (see <see cref="RefreshToken"/>).
/// A session ends when it's idle longer than the tenant's session timeout, when it reaches the
/// configured maximum length, on sign-out, or when a replaced token is used again.
/// </summary>
public sealed class SessionStore : ISessionStore
{
    /// <summary>
    /// Two tabs may refresh with the same token at the same moment. A token replaced this recently can
    /// still be exchanged once more instead of being treated as stolen.
    /// </summary>
    public static readonly TimeSpan ReuseGrace = TimeSpan.FromSeconds(30);

    public const string ReasonSignOut = "Signed out";
    public const string ReasonReuse = "Old token reused";
    public const string ReasonPasswordChanged = "Password changed";
    public const string ReasonSignOutEverywhere = "Signed out everywhere";

    private readonly ERPDbContext _db;
    private readonly ISecurityPolicyProvider _policies;
    private readonly IAuditContext? _request;
    private readonly TimeProvider _clock;

    public SessionStore(ERPDbContext db, ISecurityPolicyProvider policies, IAuditContext? request = null, TimeProvider? clock = null)
    {
        _db = db;
        _policies = policies;
        _request = request;
        _clock = clock ?? TimeProvider.System;
    }

    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public async Task<IssuedRefreshToken> StartAsync(User user, SecurityPolicy policy)
    {
        var sessionEnds = Now.AddDays(JwtHelper.RefreshTokenExpiryDays);
        var issued = Issue(user.TenantId, user.Id, Guid.NewGuid(), sessionEnds, policy);
        await _db.SaveChangesAsync();
        return issued;
    }

    public async Task<RotatedRefreshToken> RotateAsync(string token)
    {
        var hash = Hash(token);
        var current = await _db.RefreshTokens.Include(t => t.User).SingleOrDefaultAsync(t => t.TokenHash == hash)
            ?? throw new UnauthorizedException("Your session has ended. Please sign in again.");

        if (current.RevokedAt != null || current.ExpiresAt <= Now || current.SessionExpiresAt <= Now)
            throw new UnauthorizedException("Your session has ended. Please sign in again.");

        if (current.ReplacedAt != null && Now - current.ReplacedAt > ReuseGrace)
        {
            await RevokeSessionAsync(current.UserId, current.SessionId, ReasonReuse);
            throw new UnauthorizedException("Your session has ended. Please sign in again.");
        }

        var user = current.User;
        if (!user.IsActive || (user.IsLocked && user.LockedUntil > Now))
            throw new UnauthorizedException("Your account is not available. Please contact your administrator.");

        var policy = await _policies.GetAsync(user.TenantId);
        current.ReplacedAt ??= Now;
        var next = Issue(user.TenantId, user.Id, current.SessionId, current.SessionExpiresAt, policy);
        await _db.SaveChangesAsync();
        return new RotatedRefreshToken(next.Token, next.Row, user, policy);
    }

    public async Task EndAsync(string token, string reason)
    {
        var hash = Hash(token);
        var row = await _db.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.TokenHash == hash);
        if (row != null) await RevokeSessionAsync(row.UserId, row.SessionId, reason);
    }

    public async Task<int> EndAllAsync(Guid userId, string reason)
    {
        var open = await _db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync();
        foreach (var row in open) Revoke(row, reason);
        await _db.SaveChangesAsync();
        return open.Select(t => t.SessionId).Distinct().Count();
    }

    private async Task RevokeSessionAsync(Guid userId, Guid sessionId, string reason)
    {
        var rows = await _db.RefreshTokens.Where(t => t.UserId == userId && t.SessionId == sessionId && t.RevokedAt == null).ToListAsync();
        foreach (var row in rows) Revoke(row, reason);
        await _db.SaveChangesAsync();
    }

    private void Revoke(RefreshToken row, string reason)
    {
        row.RevokedAt = Now;
        row.RevokedReason = reason;
    }

    private IssuedRefreshToken Issue(Guid tenantId, Guid userId, Guid sessionId, DateTime sessionEnds, SecurityPolicy policy)
    {
        var token = JwtHelper.GenerateRefreshToken();
        var idleEnds = Now.AddMinutes(policy.SessionTimeoutMinutes);
        var row = new RefreshToken
        {
            TenantId = tenantId,
            UserId = userId,
            SessionId = sessionId,
            TokenHash = Hash(token),
            ExpiresAt = idleEnds < sessionEnds ? idleEnds : sessionEnds,
            SessionExpiresAt = sessionEnds,
            IpAddress = Truncate(_request?.IpAddress, 64),
            UserAgent = Truncate(_request?.UserAgent, 512),
            CreatedAt = Now,
        };
        _db.RefreshTokens.Add(row);
        return new IssuedRefreshToken(token, row);
    }

    private static string? Truncate(string? value, int max) => value is { Length: > 0 } && value.Length > max ? value[..max] : value;
}
