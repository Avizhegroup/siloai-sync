namespace SiloAI.Domains;

public enum AiModelKind
{
    /// <summary>Embedding / retrieval model — takes text in, produces vectors used for RAG search.</summary>
    Rag = 1,

    /// <summary>A general-purpose model assignable to a UsageFeature (chat, OCR, report, page agent).</summary>
    Normal = 2
}

/// <summary>
/// A configured AI model. Replaces the old appsettings-based OpenAI:MainModel /
/// OpenAI:VoiceModel / OpenAI:RagModel / OpenAI:EmbeddingModel / AiPricing:Models
/// configuration, which applied the same single model+price to every customer. Which model is
/// actually used for a given customer + feature is decided by <see cref="CustomerModelAssignment"/>.
/// </summary>
[Table("tbl_AiModels")]
public class AiModel
{
    [Column("fld_Id")]
    public Guid Id { get; set; }

    [Required]
    [Column("fld_Name")]
    [StringLength(200)]
    public string Name { get; set; }

    /// <summary>Exact identifier passed to the OpenAI-compatible endpoint, e.g. "openai/gpt-4.1-mini".</summary>
    [Required]
    [Column("fld_Identifier")]
    [StringLength(300)]
    public string Identifier { get; set; }

    [Required]
    [Column("fld_Kind")]
    public AiModelKind Kind { get; set; }

    // --- Normal-model I/O capabilities (meaningless / left false for Kind == Rag) ---

    [Column("fld_SupportsTextInput")]
    public bool SupportsTextInput { get; set; }

    [Column("fld_SupportsTextOutput")]
    public bool SupportsTextOutput { get; set; }

    [Column("fld_SupportsImageInput")]
    public bool SupportsImageInput { get; set; }

    [Column("fld_SupportsImageOutput")]
    public bool SupportsImageOutput { get; set; }

    [Column("fld_SupportsFileInput")]
    public bool SupportsFileInput { get; set; }

    [Column("fld_SupportsFileOutput")]
    public bool SupportsFileOutput { get; set; }

    [Column("fld_SupportsVoiceInput")]
    public bool SupportsVoiceInput { get; set; }

    [Column("fld_SupportsVoiceOutput")]
    public bool SupportsVoiceOutput { get; set; }

    // --- Rag-model specific (null / false for Kind == Normal) ---

    /// <summary>
    /// Vector width this embedding model produces. MUST match the VECTOR(N) column width
    /// declared on tbl_RagDocumentChunks.fld_Embedding (currently 1536) — changing this without
    /// also migrating that column breaks RAG search. See AiModelResolver for the runtime guard.
    /// </summary>
    [Column("fld_EmbeddingDimensions")]
    public int? EmbeddingDimensions { get; set; }

    /// <summary>
    /// The single active embedding model used for all RAG indexing/search. Only one Rag-kind
    /// model may have this set at a time (enforced by a filtered unique index).
    /// </summary>
    [Column("fld_IsDefaultRagModel")]
    public bool IsDefaultRagModel { get; set; }

    // --- Pricing (USD per 1,000,000 tokens), entered by the admin — not derived ---

    [Column("fld_InputPricePerMillionTokens")]
    public decimal InputPricePerMillionTokens { get; set; }

    [Column("fld_OutputPricePerMillionTokens")]
    public decimal OutputPricePerMillionTokens { get; set; }

    [Column("fld_CachedInputPricePerMillionTokens")]
    public decimal CachedInputPricePerMillionTokens { get; set; }

    /// <summary>Soft toggle — retire a model without breaking historical UsageRecords that reference it by name.</summary>
    [Column("fld_IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("fld_CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("fld_UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    public ICollection<CustomerModelAssignment> Assignments { get; set; } = new List<CustomerModelAssignment>();
}
