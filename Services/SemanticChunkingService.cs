using Advanced_RAG_Workshop.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Advanced_RAG_Workshop.Services;

public class ChunkingOptions
{
    public int MinChunkSize { get; set; } = 500;
    public int MaxChunkSize { get; set; } = 2000;
}

public class SemanticChunkingService
{
    private readonly IChatCompletionService _chatService;

    public SemanticChunkingService(AzureOpenAIConfig config)
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: config.DeploymentName,
                endpoint: config.Endpoint,
                apiKey: config.ApiKey)
            .Build();

        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<List<DocumentChunk>> ChunkTextAsync(string text, string sourceFile, ChunkingOptions? options = null)
    {
        options ??= new ChunkingOptions();
        
        if (string.IsNullOrWhiteSpace(text))
            return new List<DocumentChunk>();

        Console.WriteLine("🧠 Starting semantic chunking with LLM...");

        var chunks = await ChunkBySemanticBoundariesAsync(text, options);

        Console.WriteLine($"✅ Created {chunks.Count} semantic chunks");
        
        return ConvertToDocumentChunks(chunks, sourceFile);
    }

    private async Task<List<string>> ChunkBySemanticBoundariesAsync(string text, ChunkingOptions options)
    {
        var prompt = $$"""
        You are an expert at creating semantic chunks for RAG systems. 
        Your task is to split the following text into coherent chunks that keep related paragraphs and ideas together.

        RULES:
        1. Keep complete paragraphs together - never break mid-paragraph
        2. Group related paragraphs that discuss the same topic or concept
        3. Each chunk should be between {{options.MinChunkSize}} and {{options.MaxChunkSize}} characters
        4. Prefer natural topic boundaries where the subject changes
        5. Each chunk should make sense on its own

        Return the text split into chunks, separated by `---CHUNK---` markers.

        RESULT:
        - Return the chunked text with `---CHUNK---` separators
        - No additional commentary or explanation

        TEXT TO ANALYZE:
        {{text}}
        """;

        var response = await _chatService.GetChatMessageContentAsync(prompt);
        return ParseChunkedResponse(response.Content ?? "");
    }

    private List<string> ParseChunkedResponse(string response)
    {
        var chunks = new List<string>();

        var chunkTexts = response.Split(new[] { "---CHUNK---" }, StringSplitOptions.RemoveEmptyEntries);
            
        foreach (var chunk in chunkTexts)
        {
            var cleanedChunk = chunk.Trim();
            if (!string.IsNullOrWhiteSpace(cleanedChunk))
            {
                chunks.Add(cleanedChunk);
            }
        }

        if (!chunks.Any())
        {
            throw new Exception("LLM returned no chunks");
        }

        return chunks;
    }

    private List<DocumentChunk> ConvertToDocumentChunks(List<string> chunks, string sourceFile)
    {
        var documentChunks = new List<DocumentChunk>();
        
        for (int i = 0; i < chunks.Count; i++)
        {
            documentChunks.Add(new DocumentChunk
            {
                Id = $"{sourceFile}_semantic_chunk_{i}",
                Content = chunks[i],
                SourceFile = sourceFile
            });
        }
        
        return documentChunks;
    }
}
