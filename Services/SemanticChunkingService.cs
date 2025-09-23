using Advanced_RAG_Workshop.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace Advanced_RAG_Workshop.Services;

public class ChunkingOptions
{
    public int MinChunkSize { get; set; } = 500;
    public int MaxChunkSize { get; set; } = 2000;
}

// TODO milestone-2: Implement semantic chunking service using LLM to split text into meaningful chunks
public class SemanticChunkingService
{

    public SemanticChunkingService(AzureOpenAIConfig config)
    {

    }

    //public async Task<List<DocumentChunk>> ChunkTextAsync(string text, string sourceFile, ChunkingOptions? options = null)
    //{

    //}
}
