using Microsoft.Extensions.Options;
using ParsWoW.Api.Application.Abstractions.Auth;
using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Auth;
using ParsWoW.Api.Infrastructure.Auth;

namespace ParsWoW.Api.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAccountRepository _accounts;
    private readonly IPasswordHasher _hasher;
    private readonly JwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;
    private readonly AuthOptions _opts;

    public AuthService(
        IAccountRepository accounts,
        IPasswordHasher hasher,
        JwtTokenService jwt,
        IRefreshTokenStore refresh,
        IOptions<ParsWowOptions> options)
    {
        _accounts = accounts;
        _hasher = hasher;
        _jwt = jwt;
        _refresh = refresh;
        _opts = options.Value.Auth;
    }

    public async Task<Result<AuthResult>> RegisterAsync(RegisterRequest request, string ip, CancellationToken ct = default)
    {
        if (!_opts.AllowRegistration)
            return Result.Fail<AuthResult>("REGISTRATION_DISABLED", "Registration is disabled.");

        if (request.Password.Length < _opts.MinPasswordLength || request.Password.Length > _opts.MaxPasswordLength)
            return Result.Fail<AuthResult>("PASSWORD_POLICY", $"Password must be {_opts.MinPasswordLength}-{_opts.MaxPasswordLength} chars.");

        var existing = await _accounts.FindByUsernameAsync(request.Username, ct);
        if (existing is not null)
            return Result.Fail<AuthResult>("USERNAME_TAKEN", "Username is already taken.");

        var hash = _hasher.Hash(request.Username, request.Password);

        // --- Battlenet account: use BnetVerifier (SHA256) for sha_pass_hash ---
        int? bnId = null;
        try
        {
            var bnetHash = BnetVerifier.GenerateHash(request.Email, request.Password);
            bnId = await _accounts.CreateBattlenetAccountAsync(
                request.Email,
                bnetHash,
                ct);
        }
        catch
        {
            // non-fatal for emulator mode
        }

        // --- Game account: use HexVerifier (SRP6 BigInteger.ModPow) for v/s/sha ---
        var accountId = await _accounts.CreateAsync(new AccountRecord
        {
            Username = request.Username,
            Email = request.Email,
            ShaPassHash = hash.LegacyShaPassHashHex,
            VerifierHex = hash.VerifierHex,
            SaltHex = hash.SaltHex,
            BattlenetAccountId = bnId,
            LastIp = ip,
            Expansion = 1 // default expansion
        }, ct);

        var (access, jti, accessExp) = _jwt.IssueAccessToken(accountId, request.Username);
        var refreshExp = DateTimeOffset.UtcNow.AddDays(7);
        await _refresh.IssueAsync(accountId, jti, accessExp, ct);

        var refreshSigned = SignRefresh(jti, refreshExp);
        return Result.Ok(new AuthResult
        {
            AccessToken = access,
            RefreshToken = refreshSigned,
            AccessTokenExpiresAt = accessExp,
            RefreshTokenExpiresAt = refreshExp,
            Account = new AccountMeDto
            {
                Id = accountId,
                Username = request.Username,
                Email = request.Email
            }
        });
    }

    public async Task<Result<AuthResult>> LoginAsync(LoginRequest request, string ip, CancellationToken ct = default)
    {
        var account = await _accounts.FindByUsernameAsync(request.Username, ct);
        if (account is null)
            return Result.Fail<AuthResult>("INVALID_CREDENTIALS", "Invalid username or password.");

        var material = new PasswordMaterial(
            account.VerifierHex ?? string.Empty,
            account.SaltHex ?? string.Empty,
            account.ShaPassHash,
            string.Empty,
            account.VerifierHex is not null);

        if (!_hasher.Verify(request.Username, request.Password, material))
            return Result.Fail<AuthResult>("INVALID_CREDENTIALS", "Invalid username or password.");

        await _accounts.UpdateLastLoginAsync(account.Id, ip, ct);

        var (access, jti, accessExp) = _jwt.IssueAccessToken(account.Id, account.Username);
        var refreshExp = DateTimeOffset.UtcNow.AddDays(7);
        await _refresh.IssueAsync(account.Id, jti, accessExp, ct);

        return Result.Ok(new AuthResult
        {
            AccessToken = access,
            RefreshToken = SignRefresh(jti, refreshExp),
            AccessTokenExpiresAt = accessExp,
            RefreshTokenExpiresAt = refreshExp,
            Account = new AccountMeDto
            {
                Id = account.Id,
                Username = account.Username,
                Email = account.Email,
                Joindate = account.Joindate
            }
        });
    }

    public async Task<Result<AuthResult>> RefreshAsync(string refreshToken, string ip, CancellationToken ct = default)
    {
        var jti = TryExtractRefreshJti(refreshToken);
        if (jti is null)
            return Result.Fail<AuthResult>("INVALID_REFRESH", "Malformed refresh token.");

        var existing = await _refresh.GetAsync(jti, ct);
        if (existing is null || existing.IsRevoked || existing.ExpiresAt < DateTimeOffset.UtcNow)
            return Result.Fail<AuthResult>("INVALID_REFRESH", "Refresh token expired or revoked.");

        await _refresh.RevokeAsync(jti, ct);
        var account = await _accounts.FindByUsernameAsync(existing.AccountId.ToString(), ct);

        var (access, newJti, accessExp) = _jwt.IssueAccessToken(existing.AccountId, account?.Username ?? $"acct:{existing.AccountId}");
        var refreshExp = DateTimeOffset.UtcNow.AddDays(7);
        await _refresh.IssueAsync(existing.AccountId, newJti, accessExp, ct);

        return Result.Ok(new AuthResult
        {
            AccessToken = access,
            RefreshToken = SignRefresh(newJti, refreshExp),
            AccessTokenExpiresAt = accessExp,
            RefreshTokenExpiresAt = refreshExp,
            Account = new AccountMeDto
            {
                Id = existing.AccountId,
                Username = account?.Username ?? $"acct:{existing.AccountId}",
                Email = account?.Email ?? string.Empty
            }
        });
    }

    public async Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var jti = TryExtractRefreshJti(refreshToken);
        if (jti is null) return Result.Ok(true);
        await _refresh.RevokeAsync(jti, ct);
        return Result.Ok(true);
    }

    public async Task<Result<AccountMeDto>> GetCurrentAsync(int accountId, CancellationToken ct = default)
    {
        var byId = await _accounts.FindByUsernameAsync(accountId.ToString(), ct);
        if (byId is null) return Result.Fail<AccountMeDto>("NOT_FOUND", "Account not found.");
        return Result.Ok(new AccountMeDto
        {
            Id = byId.Id,
            Username = byId.Username,
            Email = byId.Email,
            Joindate = byId.Joindate
        });
    }

    private static string SignRefresh(string jti, DateTimeOffset expiresAt) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{jti}|{expiresAt.ToUnixTimeSeconds()}"));

    private static string? TryExtractRefreshJti(string token)
    {
        try
        {
            var raw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = raw.Split('|');
            return parts.Length > 0 ? parts[0] : null;
        }
        catch { return null; }
    }
}
