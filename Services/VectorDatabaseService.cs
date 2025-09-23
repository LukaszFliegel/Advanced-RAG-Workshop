using Advanced_RAG_Workshop.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;

namespace Advanced_RAG_Workshop.Services;

public class VectorSearchResult
{
    public DocumentRecord Record { get; set; } = new();
    public double Score { get; set; }
}

public class VectorDatabaseService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly InMemoryCollection<string, DocumentRecord> _collection;
    private const string CollectionName = "documents";

    public VectorDatabaseService(AzureOpenAIConfig config)
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAITextEmbeddingGeneration(
                deploymentName: config.EmbeddingDeploymentName,
                endpoint: config.Endpoint,
                apiKey: config.ApiKey)
            .Build();

        _embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        
        // Create the in-memory vector store
        var vectorStore = new InMemoryVectorStore();
        _collection = vectorStore.GetCollection<string, DocumentRecord>(CollectionName);
    }

    public async Task InitializeAsync()
    {
        // Since we have schema information available from the record definition
        // it's possible to create a collection with the right vectors, dimensions,
        // indexes and distance functions.
        await _collection.EnsureCollectionExistsAsync();
        Console.WriteLine("🧠 Vector database initialized with in-memory store");
    }

    public async Task<bool> AddDocumentsAsync(List<DocumentChunk> documents)
    {
        if (documents == null || documents.Count == 0)
        {
            return false;
        }

        Console.WriteLine("🔄 Creating embeddings and storing in vector database...");

        var tasks = new List<Task>();

        foreach (var doc in documents)
        {
            tasks.Add(ProcessDocumentAsync(doc));
        }

        await Task.WhenAll(tasks);

        Console.WriteLine($"✅ Successfully stored {documents.Count} document chunks in vector database");
        return true;
    }

    private async Task ProcessDocumentAsync(DocumentChunk doc)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(doc.Content);

        var record = new DocumentRecord
        {
            Id = doc.Id,
            Content = doc.Content,
            SourceFile = doc.SourceFile,
            DescriptionEmbedding = embedding
        };

        await _collection.UpsertAsync(record);
    }

    public async Task<List<VectorSearchResult>> SearchAsync(string query, int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<VectorSearchResult>();
        }

        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

        // Perform vector search
        var searchOptions = new VectorSearchOptions<DocumentRecord> // Specify the generic type argument
        {            
            VectorProperty = record => record.DescriptionEmbedding, // Correctly specify the vector property
        };

        var searchResults = _collection.SearchAsync(queryEmbedding, top: limit, searchOptions);
        
        var results = new List<VectorSearchResult>();
        await foreach (var result in searchResults)
        {
            results.Add(new VectorSearchResult
            {
                Record = result.Record,
                Score = result.Score ?? 0.0
            });
        }

        return results;
    }
}
