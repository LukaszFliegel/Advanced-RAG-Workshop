using Microsoft.Extensions.VectorData;

namespace Advanced_RAG_Workshop.Models;

public class DocumentRecord
{
    [VectorStoreKey]
    public string Id { get; set; } = string.Empty;

    [VectorStoreData]
    public string Content { get; set; } = string.Empty;

    [VectorStoreData]
    public string SourceFile { get; set; } = string.Empty;

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
}


public class DocumentChunk
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
}