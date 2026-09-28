namespace SiloAI.Domains;

/// <summary>
/// Which <see cref="AiModel"/> is used for a given <see cref="UsageFeature"/>. A row with
/// <see cref="CustomerId"/> = null is the global default for that feature (used when a customer
/// has no override); a row with a real CustomerId overrides the default for that one customer.
/// Only Kind == Normal models may be assigned here — the RAG/embedding model is chosen
/// separately via AiModel.IsDefaultRagModel, since it is shared infrastructure (all documents in
/// the knowledge base are embedded with the same model/dimension) rather than a per-call choice.
/// </summary>
[Table("tbl_CustomerModelAssignments")]
public class CustomerModelAssignment
{
    [Column("fld_Id")]
    public Guid Id { get; set; }

    /// <summary>Null = global default for this feature.</summary>
    [Column("fld_CustomerId")]
    public int? CustomerId { get; set; }

    [Required]
    [Column("fld_Feature")]
    public UsageFeature Feature { get; set; }

    [Required]
    [Column("fld_AiModelId")]
    public Guid AiModelId { get; set; }

    [Column("fld_CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("fld_UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    public Customer? Customer { get; set; }

    public AiModel AiModel { get; set; }
}
