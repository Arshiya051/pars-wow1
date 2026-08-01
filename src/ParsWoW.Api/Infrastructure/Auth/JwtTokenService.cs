using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParsWoW.Api.Application.Configuration;

namespace ParsWoW.Api.Infrastructure.Auth;

public sealed class JwtTokenService
{
    private readonly JwtOptions _opts;
    private readonly SigningCredentials _signing;

    public JwtTokenService(IOptions<ParsWowOptions> options)
    {
        _opts = options.Value.Jwt;
        if (string.IsNullOrEmpty(_opts.SigningKey))
            throw new InvalidOperationException("JWT signing key is not configured.");
        _signing = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public (string Token, string Jti, DateTimeOffset ExpiresAt) IssueAccessToken(int accountId, string username, IEnumerable<string>? roles = null)
    {
        var jti = Guid.NewGuid().ToString("N");
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_opts.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (roles != null)
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: _signing);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return (encoded, jti, expires);
    }

    public ClaimsPrincipal? Validate(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var tvp = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _opts.Issuer,
            ValidateAudience = true,
            ValidAudience = _opts.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signing.Key,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try { return handler.ValidateToken(token, tvp, out _); }
        catch { return null; }
    }
}
