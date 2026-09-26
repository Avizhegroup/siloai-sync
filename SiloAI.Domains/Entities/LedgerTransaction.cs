namespace SiloAI.Domains;

/// <summary>
/// Append-only ledger record — the single source of truth for every balance movement.
/// No UPDATE or DELETE is ever allowed on this table; corrections are new rows.
/// <see cref="IdempotencyKey"/> carries a UNIQUE index so a retried HTTP request can
/// never charge a customer twice.
/// </summary>
[Table("tbl_LedgerTransactions")]
public class LedgerTransaction
{
    [Key]
    [Column("fld_Id")]
    public Guid Id { get; set; }

    [Column("fld_AccountId")]
    public Guid AccountId { get; set; }

    [Column("fld_Type")]
    public LedgerTransactionType Type { get; set; }

    /// <summary>Positive = credit (TopUp/Refund), negative = debit (Usage).</summary>
    [Column("fld_Amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>Snapshot of the account balance right after this transaction.</summary>
    [Column("fld_BalanceAfter", TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }

    [Required]
    [Column("fld_IdempotencyKey")]
    [StringLength(200)]
    public string IdempotencyKey { get; set; }

    [Column("fld_UsageRecordId")]
    public Guid? UsageRecordId { get; set; }

    [Column("fld_Description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("fld_CreatedAt")]
    public DateTime CreatedAt { get; set; }

    public LedgerAccount Account { get; set; }

    public UsageRecord? UsageRecord { get; set; }
}
