namespace SiloAI.Domains;

/// <summary>
/// Per-customer financial account. <see cref="BalanceToman"/> is a cached aggregate of the
/// append-only ledger — it is only ever mutated by the credit ledger service through an
/// atomic conditional UPDATE, never written directly by handlers.
/// Each customer has exactly one account (created alongside the customer).
/// </summary>
[Table("tbl_LedgerAccounts")]
public class LedgerAccount
{
    [Key]
    [Column("fld_Id")]
    public Guid Id { get; set; }

    [Column("fld_CustomerId")]
    public int CustomerId { get; set; }

    [Column("fld_BalanceToman", TypeName = "decimal(18,2)")]
    public decimal BalanceToman { get; set; }

    [Timestamp]
    [Column("fld_RowVersion")]
    public byte[] RowVersion { get; set; }

    [Column("fld_CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("fld_UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    public Customer Customer { get; set; }

    public ICollection<LedgerTransaction> Transactions { get; set; } = new List<LedgerTransaction>();
}
