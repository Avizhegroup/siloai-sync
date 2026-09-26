namespace SiloAI.Domains;

/// <summary>
/// Historical pricing formula parameters per feature. Existing rows are never updated —
/// a price change is a new row with a later <see cref="EffectiveFrom"/>. Reads always take
/// the latest row with EffectiveFrom &lt;= now.
/// </summary>
[Table("tbl_PricingSettings")]
public class PricingSetting
{
    [Key]
    [Column("fld_Id")]
    public Guid Id { get; set; }

    [Column("fld_Feature")]
    public UsageFeature Feature { get; set; }

    /// <summary>Multiplier (M) applied to the raw toman cost.</summary>
    [Column("fld_Multiplier", TypeName = "decimal(18,4)")]
    public decimal Multiplier { get; set; }

    /// <summary>Minimum charge in toman.</summary>
    [Column("fld_FloorToman", TypeName = "decimal(18,2)")]
    public decimal FloorToman { get; set; }

    /// <summary>Minimum charge in USD (converted with the effective fx rate).</summary>
    [Column("fld_FloorUsd", TypeName = "decimal(18,4)")]
    public decimal FloorUsd { get; set; }

    [Column("fld_EffectiveFrom")]
    public DateTime EffectiveFrom { get; set; }
}
