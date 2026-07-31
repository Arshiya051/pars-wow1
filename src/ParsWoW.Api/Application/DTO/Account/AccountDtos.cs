namespace ParsWoW.Api.Application.Dto.Account;

public sealed class CharacterRenameRequest
{
    public int AccountId { get; set; }
    public Guid CharacterGuid { get; set; }
    public string NewName { get; set; } = string.Empty;
}

public sealed class RaceChangeRequest
{
    public int AccountId { get; set; }
    public Guid CharacterGuid { get; set; }
    public int NewRaceId { get; set; }
}

public sealed class FactionChangeRequest
{
    public int AccountId { get; set; }
    public Guid CharacterGuid { get; set; }
    public int NewFactionId { get; set; }
}

public sealed class AppearanceChangeRequest
{
    public int AccountId { get; set; }
    public Guid CharacterGuid { get; set; }
    public int Gender { get; set; }
    public int SkinColor { get; set; }
    public int Face { get; set; }
    public int HairStyleId { get; set; }
    public int HairColorId { get; set; }
    public int FacialFeatures { get; set; }
}

public sealed class CharacterUnstuckRequest
{
    public int AccountId { get; set; }
    public Guid CharacterGuid { get; set; }
}

public sealed class CharacterBoostRequest
{
    public int AccountId { get; set; }
    public Guid CharacterGuid { get; set; }
    public int TargetLevel { get; set; }
}

public sealed class GuildRenameRequest
{
    public int AccountId { get; set; }
    public int GuildId { get; set; }
    public string NewName { get; set; } = string.Empty;
}

public sealed class AccountOperationResultDto
{
    public bool Success { get; init; }
    public string Operation { get; init; } = string.Empty;
    public string? Message { get; init; }
    public int? AffectedEntityId { get; init; }
    public DateTimeOffset ExecutedAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
