namespace SiloAI.Domains;

/// <summary>
/// Historical exchange-rate records (toman per USD). Existing rows are never updated —
/// a rate change is a new row with a later <see cref="EffectiveFrom"/>. Reads always take
/// the latest row with EffectiveFrom &lt;= now.
/// </summary>
[Table("tbl_FxRateSettings")]
public class FxRateSetting
{
    [Key]
    [Column("fld_Id")]
    public Guid Id { get; set; }

    [Column("fld_TomanPerUsd", TypeName = "decimal(18,2)")]
    public decimal TomanPerUsd { get; set; }

    [Column("fld_EffectiveFrom")]
    public DateTime EffectiveFrom { get; set; }
}
