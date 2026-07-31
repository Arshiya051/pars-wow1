using System.ComponentModel.DataAnnotations;

namespace ParsWoW.Api.Application.Dto.Auth;

public sealed class LoginRequest
{
    [Required, StringLength(16, MinimumLength = 3)] public string Username { get; set; } = string.Empty;
    [Required, StringLength(128, MinimumLength = 1)] public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    [Required, StringLength(16, MinimumLength = 3)] public string Username { get; set; } = string.Empty;
    [Required, StringLength(128, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
}

public sealed class RefreshRequest
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}

public sealed class AuthResult
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTimeOffset AccessTokenExpiresAt { get; init; }
    public required DateTimeOffset RefreshTokenExpiresAt { get; init; }
    public required AccountMeDto Account { get; init; }
}

public sealed class AccountMeDto
{
    public int Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public DateTime? Joindate { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
