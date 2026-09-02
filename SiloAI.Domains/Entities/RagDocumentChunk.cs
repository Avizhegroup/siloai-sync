using Microsoft.Extensions.VectorData;

namespace SiloAI.Domains;

[Table("tbl_RagDocumentChunks")]
public class RagDocumentChunk
{
    [Column("fld_Id")]
    [VectorStoreKey(StorageName = "fld_Id")]
    public Guid Id { get; set; }

    [Column("fld_DocumentId")]
    [VectorStoreData(StorageName = "fld_DocumentId")]
    public Guid DocumentId { get; set; }

    public RagDocument? Document { get; set; }

    [Column("fld_ChunkIndex")]
    [VectorStoreData(StorageName = "fld_ChunkIndex")]
    public int ChunkIndex { get; set; }

    [Required]
    [Column("fld_Content")]
    [VectorStoreData(StorageName = "fld_Content")]
    public string Content { get; set; }

    [Column("fld_TokenCount")]
    public int TokenCount { get; set; }

    [Column("fld_CreateDateTime")]
    public DateTime CreateDateTime { get; set; }

    /// <summary>
    /// Stored in SQL Server 2025 as a native VECTOR(N) column. Excluded from EF model and
    /// written via raw SQL by the indexing service; read via Microsoft.Extensions.VectorData
    /// by the search service.
    /// </summary>
    [NotMapped]
    [VectorStoreVector(1536, StorageName = "fld_Embedding", DistanceFunction = "Cosine")]
    public float[]? Embedding { get; set; }
}
