using CommunityToolkit.VectorData.SqlServer;
using Microsoft.Extensions.VectorData;
using SiloAI.Application.Shared.Contracts.Rag;
using SiloAI.Domains;

namespace SiloAI.Agent.Rag;

internal sealed class RagDocumentChunkCollectionOptionsFactory(IEmbeddingService embeddings)
{
    public SqlServerCollectionOptions Create() =>
        new()
        {
            Definition = new VectorStoreCollectionDefinition
            {
                Properties =
                [
                    new VectorStoreKeyProperty(nameof(RagDocumentChunk.Id), typeof(Guid))
                    {
                        StorageName = "fld_Id"
                    },
                    new VectorStoreDataProperty(nameof(RagDocumentChunk.DocumentId), typeof(Guid))
                    {
                        StorageName = "fld_DocumentId"
                    },
                    new VectorStoreDataProperty(nameof(RagDocumentChunk.ChunkIndex), typeof(int))
                    {
                        StorageName = "fld_ChunkIndex"
                    },
                    new VectorStoreDataProperty(nameof(RagDocumentChunk.Content), typeof(string))
                    {
                        StorageName = "fld_Content"
                    },
                    new VectorStoreVectorProperty(nameof(RagDocumentChunk.Embedding), typeof(float[]), embeddings.Dimensions)
                    {
                        StorageName = "fld_Embedding",
                        DistanceFunction = DistanceFunction.CosineDistance
                    }
                ]
            }
        };
}
