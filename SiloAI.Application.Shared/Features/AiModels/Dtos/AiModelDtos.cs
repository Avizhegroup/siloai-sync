namespace SiloAI.Application.Shared.Features;

public class AiModelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }
    public AiModelKind Kind { get; set; }

    public bool SupportsTextInput { get; set; }
    public bool SupportsTextOutput { get; set; }
    public bool SupportsImageInput { get; set; }
    public bool SupportsImageOutput { get; set; }
    public bool SupportsFileInput { get; set; }
    public bool SupportsFileOutput { get; set; }
    public bool SupportsVoiceInput { get; set; }
    public bool SupportsVoiceOutput { get; set; }

    public int? EmbeddingDimensions { get; set; }
    public bool IsDefaultRagModel { get; set; }

    public decimal InputPricePerMillionTokens { get; set; }
    public decimal OutputPricePerMillionTokens { get; set; }
    public decimal CachedInputPricePerMillionTokens { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Shared shape for both create and update — an id of Guid.Empty means "create new".</summary>
public class UpsertAiModelRequest
{
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; }

    [Required, StringLength(300)]
    public string Identifier { get; set; }

    public AiModelKind Kind { get; set; }

    public bool SupportsTextInput { get; set; }
    public bool SupportsTextOutput { get; set; }
    public bool SupportsImageInput { get; set; }
    public bool SupportsImageOutput { get; set; }
    public bool SupportsFileInput { get; set; }
    public bool SupportsFileOutput { get; set; }
    public bool SupportsVoiceInput { get; set; }
    public bool SupportsVoiceOutput { get; set; }

    public int? EmbeddingDimensions { get; set; }

    [Range(0, double.MaxValue)]
    public decimal InputPricePerMillionTokens { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OutputPricePerMillionTokens { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CachedInputPricePerMillionTokens { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>One row of the assignment grid: a feature, its currently-resolved model, and whether that's an override or the default.</summary>
public class ModelAssignmentDto
{
    public UsageFeature Feature { get; set; }
    public Guid? AiModelId { get; set; }
    public string? AiModelName { get; set; }

    /// <summary>True when this came from the customer's own row rather than the global default.</summary>
    public bool IsOverride { get; set; }
}

public class SetModelAssignmentRequest
{
    /// <summary>Null means "set the global default for this feature".</summary>
    public int? CustomerId { get; set; }

    public UsageFeature Feature { get; set; }

    [Required]
    public Guid AiModelId { get; set; }
}
