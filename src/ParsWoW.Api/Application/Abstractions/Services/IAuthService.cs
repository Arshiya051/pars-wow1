using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Auth;

namespace ParsWoW.Api.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<AuthResult>> LoginAsync(LoginRequest request, string ip, CancellationToken ct = default);
    Task<Result<AuthResult>> RegisterAsync(RegisterRequest request, string ip, CancellationToken ct = default);
    Task<Result<AuthResult>> RefreshAsync(string refreshToken, string ip, CancellationToken ct = default);
    Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<Result<AccountMeDto>> GetCurrentAsync(int accountId, CancellationToken ct = default);
}
