namespace SiloAI.Agent.Rag;

/// <summary>
/// Strongly-typed configuration for the RAG knowledge base feature, bound from the
/// "RAG" section of <c>appsettings.json</c>.
/// </summary>
public class RagOptions
{
    public const string SectionName = "RAG";

    /// <summary>Target chunk size in tokens.</summary>
    public int ChunkSize { get; set; } = 800;

    /// <summary>Token overlap between adjacent chunks.</summary>
    public int ChunkOverlap { get; set; } = 100;

    /// <summary>Maximum allowed upload file size in bytes.</summary>
    public long MaxFileSize { get; set; } = 25 * 1024 * 1024;

    /// <summary>File extensions that the indexer is allowed to ingest (lower-case, includes dot).</summary>
    public string[] SupportedExtensions { get; set; } = [".txt", ".md"];
}

/// <summary>
/// Strongly-typed OpenAI configuration — provider credentials/endpoint only. Which model to use
/// for a given call (chat, RAG, OCR, embeddings) is no longer configured here: it is resolved
/// per customer/feature from the database via IAiModelResolver (see tbl_AiModels /
/// tbl_CustomerModelAssignments), so different customers and different tasks can use different
/// models with their own pricing.
/// </summary>
public class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string? ApiKey { get; set; }

    /// <summary>Optional custom endpoint (e.g. Azure OpenAI, GitHub Models gateway).</summary>
    public string? Endpoint { get; set; }
}
