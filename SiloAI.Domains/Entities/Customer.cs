namespace SiloAI.Domains;

[Table("tbl_AiCustomers")]
public class Customer
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("fld_Id")]
    public int Id { get; set; }

    [Required]
    [Column("fld_Name")]
    [StringLength(200)]
    public string Name { get; set; }

    // --- Pricing: the dollar-to-Toman rate is GLOBAL (tbl_FxRateSettings); the only per-customer
    // pricing value is this multiplier. charge = costUsd × global rate × Multiplier.
    // The credit itself lives only in the ledger (LedgerAccount.BalanceToman). ---

    /// <summary>Multiplier (ضریب) applied to the raw Toman cost of each of this customer's requests.</summary>
    [Column("fld_Multiplier", TypeName = "decimal(18,4)")]
    public decimal Multiplier { get; set; } = CustomerPricingDefaults.Multiplier;

    /// <summary>
    /// true (default): usage is refused once the balance is exhausted. false: the customer keeps
    /// using the service and the balance is allowed to go negative (it then appears in the
    /// negative-credit list on the dashboard so it can be collected).
    /// </summary>
    [Column("fld_StopOnInsufficientCredit")]
    public bool StopOnInsufficientCredit { get; set; } = true;

    [Column("fld_CreatedAt")]
    public DateTime CreatedAt { get; set; }

    public LedgerAccount? LedgerAccount { get; set; }

    public ICollection<AiApiKey> AiApiKeys { get; set; } = new List<AiApiKey>();

    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}
