namespace SiloAI.Application.Shared.Features;

public class LedgerAccountDto
{
    public Guid Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public decimal BalanceToman { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LedgerTransactionDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public LedgerTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string IdempotencyKey { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UsageRecordDto
{
    public Guid Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public UsageFeature Feature { get; set; }
    public string Model { get; set; }
    public int InputTokens { get; set; }
    public int CachedTokens { get; set; }
    public int OutputTokens { get; set; }
    public decimal CostUsd { get; set; }
    public decimal FxRateUsed { get; set; }
    public decimal MultiplierUsed { get; set; }
    public decimal FloorTomanUsed { get; set; }
    public decimal ChargeToman { get; set; }
    public Guid? ConversationId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>A customer whose ledger balance is below zero (allowed only when they are not stopped on insufficient credit).</summary>
public class NegativeCreditCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal BalanceToman { get; set; }
    public bool StopOnInsufficientCredit { get; set; }
    public DateTime UpdatedAt { get; set; }
}
