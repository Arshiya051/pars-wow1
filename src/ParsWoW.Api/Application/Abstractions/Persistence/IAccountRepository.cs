using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Application.Abstractions.Persistence;

public sealed class AccountRecord
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ShaPassHash { get; set; } = string.Empty;
    public string? VerifierHex { get; set; }
    public string? SaltHex { get; set; }
    public int? BattlenetAccountId { get; init; }
    public DateTime? Joindate { get; init; }
    public string? LastIp { get; init; }
    public int Expansion { get; init; } // stored in account.expansion column
}

public sealed class BattlenetAccountRecord
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
}

public interface IAccountRepository
{
    Task<AccountRecord?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task<int> CreateAsync(AccountRecord account, CancellationToken ct = default);
    Task UpdateLastLoginAsync(int accountId, string ip, CancellationToken ct = default);
    Task<int> CreateBattlenetAccountAsync(string email, string shaPassHash, CancellationToken ct = default);
}
