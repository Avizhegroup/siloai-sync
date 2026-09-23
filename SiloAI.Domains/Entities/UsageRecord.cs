namespace SiloAI.Domains;

/// <summary>
/// Technical detail of a single AI call. All pricing inputs (fx rate, multiplier, floor)
/// are snapshotted at charge time — never foreign keys to the settings tables — so
/// historical records stay auditable when rates change.
/// </summary>
[Table("tbl_UsageRecords")]
public class UsageRecord
{
    [Key]
    [Column("fld_Id")]
    public Guid Id { get; set; }

    [Column("fld_CustomerId")]
    public int CustomerId { get; set; }

    [Column("fld_Feature")]
    public UsageFeature Feature { get; set; }

    [Required]
    [Column("fld_Model")]
    [StringLength(200)]
    public string Model { get; set; }

    [Column("fld_InputTokens")]
    public int InputTokens { get; set; }

    [Column("fld_CachedTokens")]
    public int CachedTokens { get; set; }

    [Column("fld_OutputTokens")]
    public int OutputTokens { get; set; }

    /// <summary>Raw computed cost in USD from the model price list.</summary>
    [Column("fld_CostUsd", TypeName = "decimal(18,8)")]
    public decimal CostUsd { get; set; }

    /// <summary>Fx rate (toman per USD) that was effective at charge time.</summary>
    [Column("fld_FxRateUsed", TypeName = "decimal(18,2)")]
    public decimal FxRateUsed { get; set; }

    /// <summary>Multiplier (M) that was effective at charge time.</summary>
    [Column("fld_MultiplierUsed", TypeName = "decimal(18,4)")]
    public decimal MultiplierUsed { get; set; }

    /// <summary>Toman floor that was effective at charge time.</summary>
    [Column("fld_FloorTomanUsed", TypeName = "decimal(18,2)")]
    public decimal FloorTomanUsed { get; set; }

    /// <summary>Final result of max(cost*M, floorToman, fx*floorUsd).</summary>
    [Column("fld_ChargeToman", TypeName = "decimal(18,2)")]
    public decimal ChargeToman { get; set; }

    [Column("fld_ConversationId")]
    public Guid? ConversationId { get; set; }

    [Column("fld_CreatedAt")]
    public DateTime CreatedAt { get; set; }

    public Customer Customer { get; set; }
}
