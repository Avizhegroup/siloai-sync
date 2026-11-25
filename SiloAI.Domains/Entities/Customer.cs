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

    // --- Pricing profile: how THIS customer is billed. There is no global pricing configuration;
    // the charge for each request is max(costUsd × TomanPerUsd × Multiplier, FloorToman,
    // TomanPerUsd × FloorUsd), all taken from these columns. The credit itself lives only in the
    // ledger (LedgerAccount.BalanceToman) — there is no second balance on the customer. ---

    /// <summary>Exchange rate used to bill this customer (Toman per 1 USD).</summary>
    [Column("fld_TomanPerUsd", TypeName = "decimal(18,2)")]
    public decimal TomanPerUsd { get; set; } = CustomerPricingDefaults.TomanPerUsd;

    /// <summary>Multiplier (ضریب) applied to the raw Toman cost of each request.</summary>
    [Column("fld_Multiplier", TypeName = "decimal(18,4)")]
    public decimal Multiplier { get; set; } = CustomerPricingDefaults.Multiplier;

    /// <summary>Minimum charge per request, in Toman.</summary>
    [Column("fld_FloorToman", TypeName = "decimal(18,2)")]
    public decimal FloorToman { get; set; } = CustomerPricingDefaults.FloorToman;

    /// <summary>Minimum charge per request, in USD (converted with <see cref="TomanPerUsd"/>).</summary>
    [Column("fld_FloorUsd", TypeName = "decimal(18,4)")]
    public decimal FloorUsd { get; set; } = CustomerPricingDefaults.FloorUsd;

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
